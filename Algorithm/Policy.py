import tensorflow as tf
from tensorflow import keras

import numpy as np
import ray

class SimplePolicy:
    '''Implemented for continuous action space for DDPG only'''
    def __init__(self, network, obs_input=None, act_output=None, eps=0.01, clipped_act=1, clipped_obs=5):
    
        self.obs_input = obs_input
        self.act_output = act_output
        self.eps = eps
        self.clipped_act = clipped_act
        self.clipped_obs = clipped_obs
        
        self.network = network 
    
    def forward_network(self, inputs):
        return self.network(inputs)
    
    def forward_policy(self, inputs):
        a_kp = self.network(inputs) + np.random.rand() * self.eps
        a_kp = tf.clip_by_value(a_kp, -self.clipped_act, self.clipped_act)
        return a_kp

def c_conv_critic(input_dim, actv='relu', output_actv='sigmoid', seed_i=222, name='conv'):
    inits = keras.initializers.RandomNormal(mean=0.0, stddev=0.1, seed=seed_i) 

    ins0 = keras.layers.Input(shape=input_dim[0])
    ins1 = keras.layers.Input(shape=input_dim[1])
    
    x0 = keras.layers.Conv2D(filters=64, kernel_size=(6, 6), strides=(3, 3), activation=actv, use_bias=True,
            kernel_initializer=inits, bias_initializer='zeros')(ins0)
    x0 = keras.layers.Conv2D(filters=128, kernel_size=(4, 4), strides=(3, 3), activation=actv, use_bias=True,
            kernel_initializer=inits, bias_initializer='zeros')(x0)
    x0 = keras.layers.Conv2D(filters=128, kernel_size=(4, 4), strides=(3, 3), activation=actv, use_bias=True,
            kernel_initializer=inits, bias_initializer='zeros')(x0)
    x0 = keras.layers.Flatten()(x0)
    
    x1 = keras.layers.Dense(128, activation=actv, use_bias=True,
            kernel_initializer=inits, bias_initializer='zeros')(ins1)
    x1 = keras.layers.Dense(128, activation=actv, use_bias=True,
            kernel_initializer=inits, bias_initializer='zeros')(x1)
    

    x = keras.layers.Concatenate(axis=-1)([x0, x1])
    x = keras.layers.Dense(64, activation=actv)(x)
    out = keras.layers.Dense(1, activation=output_actv)(x)
    
    return keras.Model([ins0, ins1], out)

def c_conv_actor(input_dim, output_dim, actv='relu', output_actv='sigmoid', seed_i=222, name='conv'):
    inits = keras.initializers.RandomNormal(mean=0.0, stddev=0.1, seed=seed_i) 

    ins = keras.layers.Input(shape=input_dim)
    x = keras.layers.Conv2D(filters=128, kernel_size=(6, 6), strides=(3, 3), activation=actv, use_bias=True,
            kernel_initializer=inits, bias_initializer='zeros')(ins)
    x = keras.layers.Conv2D(filters=128, kernel_size=(4, 4), strides=(3, 3), activation=actv, use_bias=True,
            kernel_initializer=inits, bias_initializer='zeros')(x)
    x = keras.layers.Conv2D(filters=200, kernel_size=(4, 4), strides=(1, 1), activation=actv, use_bias=True,
            kernel_initializer=inits, bias_initializer='zeros')(x)
    x = keras.layers.Flatten()(x)
    x = keras.layers.Dense(256, activation=actv)(x)
    out = keras.layers.Dense(output_dim, activation=output_actv)(x)
    
    return keras.Model(ins, out)

def dense_rs(x, n_hidden=3, size=128, act='sigmoid', b=True):
    for i in range(n_hidden):
        x = tf.keras.layers.Dense(size, activation=act, use_bias=b)(x)
    return x

def create_dense_inp(ins, outs, hidden=128, n_hidden=3, 
                     act='sigmoid', out_act='sigmoid', b=True, concat=-1):
    ip0 = [tf.keras.layers.Input(shape=(ik, )) for ik in ins]
    ds0 = [tf.keras.layers.Dense(hidden, activation=act, use_bias=b)(xk) for xk in ip0]
    c_ = tf.keras.layers.Concatenate(-1)(ds0)
    ds1 = dense_rs(c_, n_hidden, size=hidden, act=act)
    op0 = [tf.keras.layers.Dense(od, activation=act, use_bias=b)(ds1) for od in outs]
    if concat:
        c_ = tf.keras.layers.Concatenate(concat)(op0)
        return tf.keras.Model(ip0, c_)
    return tf.keras.Model(ip0, op0)
