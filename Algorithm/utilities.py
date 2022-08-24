import numpy as np
import tensorflow as tf


def reverse_var_shape(var_flatten, net0):
    var_flatten = np.array(var_flatten)
    v = []
    prods = 0
    for each_layer in net0.get_weights():
        shape= each_layer.shape
        prods0 = int(prods + np.prod(shape))
        v.append(var_flatten[prods:prods0].reshape(shape))
        prods = prods0
    return v 

def set_from_flat(var_flatten, net0):
    net0.set_weights(reverse_var_shape(var_flatten, net0))

def flatten_shape(xs):
    return tf.concat([tf.reshape(x, (-1,)) for x in xs], axis=0)

def validate_params(xs):
    for xk in xs:
        if xs is None or tf.math.is_nan(xs):
            raise ValueError
        