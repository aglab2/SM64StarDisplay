SRC_DIR = src
OBJ_DIR = obj

# Alter these 4 variables according to your need
CPP_FILES = main net
ROM = ../netbin.bs
INCLUDE_PATH = C:\sm64-api\sm64
LIBRARY_PATH = C:\sm64-api\ld
LIBC_PATH = C:\sm64-api\sm64\libc

# 0x0
ROM_OFFSET = 0

SRC_FILES = $(patsubst %, $(SRC_DIR)/%.cpp, $(CPP_FILES))
OBJ_FILES = $(patsubst %, $(OBJ_DIR)/%.o, $(CPP_FILES))

CC = clang++
AR = llvm-ar
LD = ld.lld
CFLAGS = -Wall -Wdouble-promotion -Os -march=mips2 -mno-check-zero-division --target=mips-unknown-n32 -fomit-frame-pointer -G0 -I $(INCLUDE_PATH) -I $(LIBC_PATH) -fno-exceptions -fno-builtin -fno-rtti -fno-common -mno-abicalls -flto -DTARGET_N64 -mfpxx

all: $(OBJ_DIR) inject

$(OBJ_DIR)/%.o: $(SRC_DIR)/%.cpp
	$(CC) $(CFLAGS) $< -c -o $@
	
inject: $(OBJ_FILES)
	$(LD) -o tmp -L. -L $(LIBRARY_PATH) --oformat binary -T ldscript -M $^
	dd bs=1 seek=$(ROM_OFFSET) if=tmp of=$(ROM) conv=notrunc
	rm tmp

$(OBJ_DIR):
	mkdir $(OBJ_DIR)

clean:
	rm -rf $(OBJ_DIR)
