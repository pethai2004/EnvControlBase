from google.cloud import storage

import zipfile
import os
import pathlib
import copy 

import ray 
from mlagents_envs.environment import UnityEnvironment, ActionTuple

import tensorflow as tf
import numpy as np
from collections import namedtuple

from utilities import reverse_var_shape, flatten_shape, set_from_flat, validate_params
Transitions = namedtuple('Transitions', ['states', 'actions', 'rewards', 'next_states'])
GradsType = namedtuple('Transitions', ['policy', 'value'])

def download_public_file(bucket_name, source_blob_name, destination_file_name, remove_download=True):
    # example : download_public_file('builtbuck001', 'ENV02_RunMarathon.zip', 'loadedfile')
    storage_client = storage.Client.create_anonymous_client()
    bucket = storage_client.bucket(bucket_name)
    blob = bucket.blob(source_blob_name)
    blob.download_to_filename(destination_file_name)
    print("completed download from bucket")
    path_to_zip_file  = os.path.join(os.getcwd(), destination_file_name)
    
    with zipfile.ZipFile(path_to_zip_file, 'r') as zip_ref:
        zip_ref.extractall(os.path.dirname(path_to_zip_file))
    print("extracted to local machine : ", path_to_zip_file)
    
    if remove_download:
        os.remove(destination_file_name)

class Buffer:
    def __init__(self, max_memory=500_000, num_pop_out=100_000):

        self.states = []
        self.next_states = []
        self.actions = []
        self.rewards = []
        self.curr_idx = 0
        self.max_memory = max_memory
        self.memory = max_memory # this will keep change after pop out  when idx more than max_memory / minimum memory to store in buffer
        self.num_pop_out = num_pop_out

    def store_transition(self, S, A, R, Skp):
        self.curr_idx += 1

        self.states.append(S) 
        self.next_states.append(Skp)
        self.actions.append(A)
        self.rewards.append(R)

        if self.curr_idx >= self.max_memory:
            curr_idx = 0
            self.memory = self.max_memory - self.num_pop_out
            self.clean_up() # clean up only first self.num_pop_out

    def clean_up(self, slices=None):
        slices = slices if slices is not None else self.num_pop_out

        del self.states[:slices]
        del self.next_states[:slices]
        del self.rewards[:slices]
        del self.actions[:slices]

    def get_raw(self):
        return (self.states, self.actions, self.rewards, self.next_states)

class SimulatorManager:
    '''Manager handling Envsimulator Workers'''
    '''Inputs :
            (str) env_path : path to env
            (function) policy_func : function that return tensorflow model (policy model)
    '''
    def __init__(self, env_path, policy, worker_number=100, batch_size=10000, buffer_size=50000, max_episode=100, max_env_steps=10000, buffer_popout=40000
            ,seed_base=5050):

        self.seed_base = seed_base
        self.env_path = env_path
        self.worker_number = worker_number
        self.workers = []
        self.policy = policy
        self.batch_size = batch_size
        self.buffer_size = buffer_size
        self.buffer_popout = buffer_popout
        self.max_episode = max_episode
        self.max_env_steps = max_env_steps
        self.global_env_steps = tf.Variable(0, dtype=tf.int64, name='global_env_steps')
        self.global_buffer = None
        self.p_lr = 0.00001
        self.q_lr = 0.001
        self.policy_updater = None # define either update function class or TF optimizer
        self.value_updater = None
        self.global_q_net = None
        self.global_p_net = None

        self.summarizer = None
        assert (batch_size < buffer_size and buffer_popout < buffer_size)

    def get_params_update(self, wait_workers=False):
        '''Get all net params update rule from all workers
        Input:
            wait_workers (bool) : Whether to receive update rule immediately if it is available or wait until all workers are done.
        '''
        if not wait_workers:
            result_comput = ray.get([self.workers[i].get_all_direction.remote(re_rollout=True, get_flatten=True)
                                            for i in range(self.worker_number)])
        else:
            raise NotImplementedError
        Grads = [I[0] for I in result_comput]
        grad_info = [I[1] for I in result_comput]

        p_grads = reverse_var_shape(tf.math.reduce_sum([g.policy for g in Grads], axis=0), self.global_p_net)
        q_grads = reverse_var_shape(tf.math.reduce_sum([g.value for g in Grads], axis=0), self.global_q_net)

        if (self.policy_updater is not None and self.value_updater is not None):
            self.policy_updater.apply_gradients(zip(p_grads, self.global_p_net.trainable_variables))
            self.value_updater.apply_gradients(zip(q_grads, self.global_q_net.trainable_variables))

        self.set_synced(p_net=self.global_p_net.get_weights(), q_net=self.global_q_net.get_weights())

        summarize_grad_p = tf.reduce_mean([n_g[0] for n_g in grad_info])
        summarize_grad_w = tf.reduce_mean([n_g[1] for n_g in grad_info])

    def kill_all(self):
        for each_w in range(self.worker_number):
            ray.kill(self.workers[each_w])

    def get_global_step(self):
        self.global_env_steps = ray.get([self.workers[i].get_step.remote() for i in range(self.worker_number)])
        return self.global_env_steps

    def get_global_buffer(self):
        '''Append every Buffer from all workers in one single place'''
        results = ray.get([self.workers[i].get_achieved.remote(norm_state=False, norm_reward=False, dtype=tf.float32, shuf_seed=None) 
                                for i in range(self.worker_number)])
        return results

    def set_synced(self, p_net, q_net):

        ID_runs = [self.workers[i].set_synced.remote(p_net, q_net) for i in range(self.worker_number)]
        ray.get(ID_runs)

    def initialize_workers(self, worker_id_offset=1, no_graphics=False, base_port=5020, seed_offset=1000):
        '''Initialize Worker'''
        self.workers = [DPGSim.remote(env_path=self.env_path, 
                            policy=self.policy, 
                            batch_size=self.batch_size, 
                            buffer_size=self.buffer_size, 
                            num_pop_out=self.buffer_popout, 
                            worker_id=i + worker_id_offset, 
                            no_graphics=no_graphics, 
                            base_port=base_port+i, 
                            max_episode=self.max_episode, 
                            max_env_steps=10000, 
                            env_seed=seed_offset + i, 
                            p_lr=self.p_lr, 
                            q_lr=self.q_lr) 
                                for i in range(self.worker_number)]

    def initialze_workers_net(self, p_net, q_net):
        self.global_p_net = p_net
        self.global_q_net = q_net
        ID_runs = [self.workers[i].init_target_net.remote(net_crit=self.global_q_net, net_act=self.global_p_net) 
                                                for i in range(self.worker_number)]
        ray.get(ID_runs)

    def receive_rollouts(self):
        '''Tel worker to do episode rollouts, this is not computation of gradient of updating network weights'''
        ID_list = []
        for each_w in range(self.worker_number):
            id_l = self.workers[each_w].do_rollouts.remote()
            ID_list.append(id_l)
        return ID_list

class UnityEnvSimulator:
    '''Distribute for multiple workers'''

    def __init__(self, env_path, policy, batch_size=10000, buffer_size=50_000, num_pop_out=10_000, worker_id=0, no_graphics=True, base_port=5020, max_episode=100, 
            max_env_steps=10000, env_seed=4040, log_env_folder=""):
        
        self.env_path = env_path
        self.env_seed = env_seed
        self.worker_id = worker_id
        self.base_port = base_port
        self.log_env_folder = log_env_folder
        self.env_unity = UnityEnvironment(env_path, worker_id=worker_id, base_port=base_port, seed=self.env_seed,
            no_graphics=no_graphics, log_folder=self.log_env_folder)

        self.policy = policy
        self.max_episode = max_episode
        self.max_env_steps = max_env_steps
        self.env_unity.reset()
        assert len(list(self.env_unity.behavior_specs)) >= 0, 'only support one behavior_name'
        self.behavior_name = list(self.env_unity.behavior_specs)[0]
        self.env_spec = self.env_unity.behavior_specs[self.behavior_name]
        self.buffer_size = buffer_size
        self.num_pop_out = num_pop_out
        self.Buffer = Buffer(max_memory=buffer_size, num_pop_out=num_pop_out)
        self.g_step = tf.Variable(0, dtype=tf.int64, name="Worker_steps ID : {}".format(str(worker_id)))
        self.batch_size = batch_size    
        self.curr_mean_r = []
        self.curr_mean_step = []
        assert num_pop_out < self.buffer_size

    def get_step(self):
        return {str('Worker : ' + self.worker_id) :self.g_step}

    def get_mean_reward(self):
        return self.curr_mean_r, self.curr_mean_step

    def do_rollouts(self):
        '''Do single rollouts of one worker'''
        for i in range(self.max_episode):
            self.env_unity.reset()
            decision_steps, terminal_steps = self.env_unity.get_steps(self.behavior_name)
            self.g_step.assign_add(1)
            S_k =  decision_steps.obs
            Reward_hist, Step_hist = 0, 0
            for t in range(self.max_env_steps):

                a_kk = self.policy.forward_policy(S_k)
                act_tuple = ActionTuple()
                act_tuple.add_continuous(a_kk)

                self.env_unity.set_actions(self.behavior_name, act_tuple)
                self.env_unity.step()
                
                decision_steps, terminal_steps = self.env_unity.get_steps(self.behavior_name)
                S_kk = decision_steps.obs
                R_k = decision_steps.reward

                if len(list(terminal_steps)) >=1 or Step_hist >= self.max_env_steps:
                    print("break")
                    R_k = terminal_steps.reward
                    S_kk = terminal_steps.obs
                    self.Buffer.store_transition(S_k, a_kk, R_k, S_kk)

                    self.curr_mean_r.append(Reward_hist)
                    self.curr_mean_step.append(Step_hist)
                    Reward_hist = 0
                    Step_hist = 0
                    break
                    
                self.Buffer.store_transition(S_k, a_kk, R_k, S_kk)
                self.g_step.assign_add(1) # add global env step
                Reward_hist += R_k
                Step_hist += 1
                S_k = S_kk  


    def get_achieved(self, norm_state=False, norm_reward=False, dtype=tf.float32, shuf_seed=None):
        '''Get all results from all workers'''
        
        if shuf_seed is None:
            shuf_seed = np.random.randint(1000) # random shuffle seed for seeding idx
        buff_size = self.buffer_size if self.buffer_size <= self.Buffer.memory else self.Buffer.memory
        buff_size = min(self.g_step.numpy(), buff_size)
        assert buff_size != 0
        batch = self.batch_size if self.batch_size < buff_size else buff_size
        assert self.g_step >= batch, "Could cal 'get_achieved' since 'g_step' is still less than buffer_size"
        print(self.buffer_size, self.Buffer.memory, self.g_step, batch)
        np.random.seed(shuf_seed)
        idxes = np.random.choice(buff_size, (batch), replace=False)

        S_b = tf.gather(tf.squeeze(self.Buffer.states, name='states'), indices=idxes)
        A_b = tf.gather(tf.squeeze(self.Buffer.actions, name='actions'), indices=idxes)
        R_b = tf.gather(tf.squeeze(self.Buffer.rewards, name='rewards'), indices=idxes)
        Sp_b = tf.gather(tf.squeeze(self.Buffer.next_states, name='next_states'), indices=idxes)
        
        if norm_state:
            S_b = (S_b - tf.math.reduce_mean(S_b)) /( tf.math.reduce_std(S_b) + 0.0000001)
            Sp_b = (Sp_b - tf.math.reduce_mean(Sp_b)) /( tf.math.reduce_std(Sp_b) + 0.0000001)
        if norm_reward:
            R_b = (R_b - tf.math.reduce_mean(R_b)) / (tf.math.reduce_std(R_b) + 0.0000001)

        return Transitions(S_b, A_b, R_b, Sp_b)

    def set_synced(self, p_net, q_net,):
        '''Get any available weights from other server and set it to this server'''
        return self._set_synced(p_net, q_net)
    def _set_synced(self):
        raise NotImplementedError
        
    def get_synced(self):
        '''Return any available weights from this worker to other server'''
        return self._get_synced()
    def _get_synced(self):
        raise NotImplementedError
        
    def get_all_direction(self, **kwargs):
        '''Return any available direction to be used in update'''
        return self._get_all_direction(*kwargs)
    def _get_all_direction(self):
        raise NotImplementedError

    def __repr__(self):
        return str('Worker : ' + self.worker_id)

@ray.remote
class DPGSim(UnityEnvSimulator):

    def __init__(self, env_path, policy, batch_size=10000, buffer_size=50_000, num_pop_out=10_000, worker_id=0, no_graphics=True, base_port=5020, max_episode=100, 
            max_env_steps=10000, env_seed=4040, log_env_folder="", p_lr=0.00001, q_lr=0.001):

        super().__init__(env_path, policy, batch_size, buffer_size, num_pop_out, worker_id, no_graphics, base_port, max_episode, 
            max_env_steps, env_seed, log_env_folder)

        self.target_q_network = None
        self.q_network = None
        self.target_policy = None
        self.policy = policy
        self.gamma = 0.995
        self.grad_clip = True
        self.p_lr = p_lr
        self.q_lr = q_lr
        
    def init_target_net(self, net_crit, net_act):
        
        self.q_network = copy.deepcopy(net_crit)
        self.target_q_network = copy.deepcopy(net_crit)
        self.policy.network = copy.deepcopy(net_act)
        self.target_policy = copy.deepcopy(self.policy)
            
    def forward_p_grad(self, S):

        with tf.GradientTape() as tape0:
            a_ = self.policy.forward_network(S)
            q_ = self.q_network([S, a_])
            p_loss = - tf.math.reduce_mean(q_)

        p_grad = tape0.gradient(p_loss, self.policy.network.trainable_variables)
        if self.grad_clip:
            p_grad, p_grad_norm = tf.clip_by_global_norm(p_grad, self.grad_clip)
        else: 
            p_grad_norm = 0
        return p_grad, p_grad_norm

    def forward_q_grad(self, S, R, A, Splus):

        with tf.GradientTape() as tape1:
            a_tk = self.policy.forward_network(Splus)
            y_q = R + self.gamma * self.target_q_network([Splus, a_tk])
            x_q = self.q_network([S, A]) 
            q_loss = tf.math.reduce_mean(tf.math.squared_difference(y_q, x_q))

        q_grad = tape1.gradient(q_loss, self.q_network.trainable_variables)
        if self.grad_clip:
            q_grad, q_grad_norm = tf.clip_by_global_norm(q_grad, self.grad_clip)
        else: 
            q_grad_norm = 0
        return q_grad, q_grad_norm
    
    def update_target(self):
        raise NotImplementedError

    def _get_synced(self):
        return self.target_policy, self.target_q_network

    def _set_synced(self, p_net, q_net):
        self.target_policy.network.set_weights(p_net)
        self.target_q_network.set_weights(q_net)

    def _get_all_direction(self, re_rollout=True, get_flatten=True):
        if re_rollout:
            self.do_rollouts()
        TRJ_result = self.get_achieved(norm_state=True, norm_reward=True, dtype=tf.float32, shuf_seed=None)

        f_p_grad, f_pn = self.forward_p_grad(S=TRJ_result.states)
        f_q_grad, q_qn = self.forward_q_grad(S=TRJ_result.states, 
                                             R=TRJ_result.rewards, 
                                             A=TRJ_result.actions,
                                             Splus=TRJ_result.next_states)
        if get_flatten:
            f_p_grad = flatten_shape(f_p_grad) 
            f_q_grad = flatten_shape(f_q_grad) 

        return GradsType(f_p_grad, f_q_grad), (f_pn, q_qn)

