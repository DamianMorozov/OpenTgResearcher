"""Создаёт синтетические settingss по формату Telegram Desktop без пользовательских данных"""

import hashlib
import struct
from pathlib import Path

from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes

# Структуры сверены с telegramdesktop/tdesktop, ревизия 5ff615713d2178b8458e201778d23d7abe78db9b
# Файлы: storage/details/storage_file_utilities.cpp, core/core_settings.cpp, core/core_settings_proxy.cpp
ROOT = Path(__file__).parent
VERSION = 6004001


def integer(value):
    return struct.pack(">i", value)


def blob(value):
    return integer(len(value)) + value


def string(value):
    return blob(value.encode("utf-16-be"))


def proxy(kind, host, port, user="", password=""):
    return integer(kind) + string(host) + integer(port) + string(user) + string(password)


def application(proxies):
    # Непустые переменные поля перед прокси обнаруживают ошибочные фиксированные смещения
    data = blob(b"accent") + integer(1) * 5 + string("D:/Downloads") + blob(b"bookmark")
    data += integer(1) * 9 + string("output") + string("input") + integer(1) * 4
    data += integer(1) + string("sound") + string("path")
    data += integer(1) * 13 + blob(b"pip") + integer(1) + struct.pack(">Q", 42)
    data += integer(1) * 12 + string("camera") + integer(1) * 2 + blob(b"shortcut")
    data += struct.pack(">q", 200) + integer(0) * 2 + blob(b"window")
    data += integer(1) + string("emoji") + struct.pack(">H", 3)
    data += integer(1) + string("variant") + bytes([2]) + integer(0) * 3
    selected = proxies[0] if proxies else proxy(0, "", 0)
    settings = integer(1) + integer(0) + integer(2) + blob(selected) + integer(len(proxies))
    settings += b"".join(blob(item) for item in proxies) + integer(1)
    return data + blob(settings) + integer(0) * 3


def encrypt(data):
    salt = bytes(range(32))
    auth = hashlib.pbkdf2_hmac("sha1", b"", salt, 4, 256)
    plain = struct.pack("<I", len(data) + 4) + data
    plain += bytes((-len(plain)) % 16)
    msg = hashlib.sha1(plain).digest()[:16]
    a = hashlib.sha1(msg + auth[8:40]).digest()
    b = hashlib.sha1(auth[40:56] + msg + auth[56:72]).digest()
    c = hashlib.sha1(auth[72:104] + msg).digest()
    d = hashlib.sha1(msg + auth[104:136]).digest()
    key = a[:8] + b[8:20] + c[4:16]
    previous_cipher = a[8:20] + b[:4]
    previous_plain = b[4:8] + c[16:20] + d[:8]
    aes = Cipher(algorithms.AES(key), modes.ECB()).encryptor()
    encrypted = bytearray(msg)
    for offset in range(0, len(plain), 16):
        block = plain[offset:offset + 16]
        mixed = bytes(x ^ y for x, y in zip(block, previous_cipher))
        encoded = bytes(x ^ y for x, y in zip(aes.update(mixed), previous_plain))
        encrypted.extend(encoded)
        previous_cipher, previous_plain = encoded, block
    return salt, bytes(encrypted)


def container(salt, encrypted):
    payload = blob(salt) + blob(encrypted)
    version = struct.pack("<i", VERSION)
    signature = hashlib.md5(payload + struct.pack("<i", len(payload)) + version + b"TDF$").digest()
    return b"TDF$" + version + payload + signature


def write_fixture(name, data):
    salt, encrypted = encrypt(data)
    (ROOT / name).write_bytes(container(salt, encrypted))


proxies = [proxy(1, "socks.example.test", 1080, "Тест", "pass:123"),
           proxy(2, "2001:db8::1", 65535), proxy(3, "mtproto.example.test", 443, "", "dd" + "ab" * 16)]
blocks = integer(0x06) + integer(1) + integer(0x60) + blob(b"fallback")
blocks += integer(0x5E) + blob(application(proxies)) + integer(0x54) + bytes(20)
write_fixture("modern.tdf", blocks)
write_fixture("empty.tdf", integer(0x5E) + blob(application([])))
write_fixture("invalid-proxy.tdf", integer(0x5E) + blob(application([proxies[0], proxy(1, "bad host", 0)])))
write_fixture("unsupported.tdf", integer(0x7FFFFFFE) + integer(0))
write_fixture("truncated.tdf", integer(0x5E) + blob(application(proxies)[:80]))
web = proxy(4, "web.example.test", 443, "/bridge", "test-web-secret")
write_fixture("mixed-web.tdf", integer(0x5E) + blob(application([web, proxies[0], web, proxies[1], proxies[2], web])))
write_fixture("only-web.tdf", integer(0x5E) + blob(application([web])))
write_fixture("invalid-after-web.tdf", integer(0x5E) + blob(application([web, proxies[0], proxy(2, "bad.example.test", 0)])))
write_fixture("unknown-proxy.tdf", integer(0x5E) + blob(application([proxy(5, "unknown.example.test", 443)])))
salt, encrypted = encrypt(blocks)
damaged = bytearray(encrypted)
damaged[-1] ^= 1
(ROOT / "bad-encryption.tdf").write_bytes(container(salt, damaged))
