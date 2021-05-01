data = ''
with open('bzero.txt', 'r') as f:
    data = f.read().split(' ')

with open('bzero-int.txt', 'w') as f:
    val = '0x'
    idx = 0
    for b in data:
        idx = idx + 1
        val = val + b
        if idx % 4 == 0:
            f.write(val)
            f.write(', ')
            val = '0x'