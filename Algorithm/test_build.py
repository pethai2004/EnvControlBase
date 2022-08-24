from base_env_builts import SimulatorManager, UnityEnvSimulator, Buffer
from Policy import SimplePolicy

import ray 
from tensorflow import keras
env_path = "/Users/pethai/Desktop/env_project001/Builts/macOS/ENV02_RunMarathon/basicrun001macnocam"

network = network = keras.Sequential([
    keras.layers.Input(shape=(None, 136)),
    keras.layers.Dense(128,),
    keras.layers.Dense(39)
    
])
policy = SimplePolicy.remote(network=network)

manager = SimulatorManager(env_path=env_path, policy=policy, worker_number=3, batch_size=200, buffer_size=500, max_episode=4, max_env_steps=400, 
	seed_base=5050)
manager.initialize_workers(worker_id_offset=1, no_graphics=False, base_port=5020, seed_offset=1000)

obj_ref = manager.receive_rollouts()
ray.wait(obj_ref)
print(manager.global_env_steps)
# result = manager.get_achieved()

# for i in result:
# 	print(result.shape)