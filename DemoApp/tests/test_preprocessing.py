import numpy as np
import pytest
from utils.image_utils import decode_image, preprocess


def test_decode_image_valid(sample_image_bytes):
    img = decode_image(sample_image_bytes)
    assert img is not None
    assert len(img.shape) == 3  # H x W x C


def test_decode_image_invalid_raises():
    with pytest.raises(ValueError):
        decode_image(b"not an image")


def test_preprocess_returns_tuple(sample_image_bytes):
    img, size = preprocess(sample_image_bytes)
    assert isinstance(img, np.ndarray)
    assert isinstance(size, tuple)
    assert len(size) == 2
    assert size[0] > 0 and size[1] > 0


def test_preprocess_returns_color_image(sample_image_bytes):
    img, _ = preprocess(sample_image_bytes)
    assert len(img.shape) == 3
    assert img.shape[2] == 3  # BGR channels preserved — not grayscale
