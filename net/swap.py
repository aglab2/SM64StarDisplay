import struct

lst = None
with open('netbin.bs', 'rb') as f:
    data = f.read()
    fmt = f'>{len(data) // 4}I'
    lst = struct.unpack(fmt, data)

with open('netbin', 'wb') as f:
    fmt = f'<{len(lst)}I'
    data = struct.pack(fmt, *lst)
    f.write(data)
