/*
 Copyright (c) num0005. Some rights reserved
 This software is part of the Osoyoos Launcher.
 Released under the MIT License, see LICENSE.md for more information.
*/


#pragma once
#include "platform.h"

/*
	Writes `numBytes` bytes from `patch` to `destAddress`
*/
inline void WriteBytes(void* destAddress, const void* patch, size_t numBytes)
{
	if (destAddress && patch && numBytes > 0)
	{
		DWORD OldProtection;

		VirtualProtect(destAddress, numBytes, PAGE_EXECUTE_READWRITE, &OldProtection);
		memcpy(destAddress, patch, numBytes);
		VirtualProtect(destAddress, numBytes, OldProtection, &OldProtection);

		FlushInstructionCache(GetCurrentProcess(), destAddress, numBytes);
	}
}

/*
	Writes `numBytes` bytes from `patch` to `destAddress`
*/
inline void WriteBytes(size_t destAddress, const void* patch, size_t numBytes)
{
	WriteBytes(reinterpret_cast<void*>(destAddress), patch, numBytes);
}

/*
	Writes an array into memory
*/
template<typename t, size_t size>
inline void  WriteArray(size_t address, t(*data)[size])
{
	WriteBytes(address, data, sizeof(t) * size);
}

/*
	Writes an array into memory
*/
template<typename t, size_t size>
inline void  WriteArray(void* address, t(*data)[size])
{
	WriteBytes(address, data, sizeof(t) * size);
}

/*
	Writes data to memory at address
*/
template <typename value_type>
inline void WriteValue(size_t address, value_type data)
{
	WriteBytes(address, &data, sizeof(data));
}

/*
Writes data to memory at address
*/
template <typename value_type>
inline void WriteValue(void* address, value_type data)
{
	WriteBytes(address, &data, sizeof(data));
}


/*
	Writes pointer to memory address
*/
inline void WritePointer(size_t offset, const void* ptr) {
	WriteValue(offset, ptr);
}

/*
	Writes pointer to memory address
*/
inline void WritePointer(void* offset, const void* ptr) {
	WriteValue(reinterpret_cast<size_t>(offset), ptr);
}




/*
	Write a block of `len` of nops at `address`
*/
__forceinline void NopFill(const size_t address, int len)
{
	BYTE nop_fill_small[0x100];
	BYTE* nop_fill = nullptr;
	if (len > ARRAYSIZE(nop_fill_small))
		nop_fill = new BYTE[len];
	else
		nop_fill = nop_fill_small;

	memset(nop_fill, 0x90, len);
	WriteBytes(address, nop_fill, len);

	if (nop_fill != nop_fill_small) // free if not stack allocation
		delete[] nop_fill;
}

/*
	Write a block of `len` of nops at `address`
*/
inline void NopFill(const void* address, int len)
{
	NopFill(reinterpret_cast<size_t>(address), len);
}

inline void NopFillRange(const size_t address_start, const size_t address_end)
{
	NopFill(address_start, address_end - address_start);
}

inline void NopFillRange(const void* address_start, const void* address_end)
{
	NopFillRange(reinterpret_cast<size_t>(address_start), reinterpret_cast<size_t>(address_end));
}




/*
	Patches an existing function call
*/
inline void PatchCall(size_t call_addr, size_t new_function_ptr) {
	size_t callRelative = new_function_ptr - (call_addr + 5);
	WriteValue(call_addr + 1, reinterpret_cast<void*>(callRelative));
}

/*
	Patches an existing function call
*/
inline void PatchCall(size_t call_addr, void* new_function_ptr)
{
	PatchCall(call_addr, reinterpret_cast<size_t>(new_function_ptr));
}



/*
	Write relative jump at `address` to `target_addr`
*/
inline void WriteJmp(size_t call_addr, size_t target_addr)
{
	BYTE call_patch[1] = { 0xE9 };
	WriteBytes(call_addr, call_patch, 1);
	PatchCall(call_addr, target_addr);
}

/*
	Write relative jump at `address` to `target_addr`
*/
inline void WriteJmp(size_t address, void* target_addr)
{
	WriteJmp(address, reinterpret_cast<size_t>(target_addr));
}

/*
	Write relative jump at `address` to `target_addr`
*/
inline void WriteJmp(void* address, void* target_addr)
{
	WriteJmp(reinterpret_cast<size_t>(address), reinterpret_cast<size_t>(target_addr));
}



/*
	Write call to `function_ptr` at `address`
*/
inline void WriteCall(size_t address, size_t function_ptr)
{
	BYTE call_patch[1] = { 0xE8 };
	WriteBytes(address, call_patch, 1);
	PatchCall(address, function_ptr);
}

/*
	Write call to `function_ptr` at `address`
*/
inline void WriteCall(size_t address, void* function_ptr)
{
	WriteCall(address, reinterpret_cast<size_t>(function_ptr));
}

/*
	Write call to `function_ptr` at `address`
*/
inline void WriteCall(void* address, void* function_ptr)
{
	WriteCall(reinterpret_cast<size_t>(address), reinterpret_cast<size_t>(function_ptr));
}



/*
	Patches an absolute call
*/
inline void PatchAbsCall(size_t call_addr, size_t new_function_ptr)
{
	WriteCall(call_addr, new_function_ptr);
	NopFill(call_addr + 5, 1);
}

/*
	Patches an absolute call
*/
inline void PatchAbsCall(void* call_addr, void* new_function_ptr)
{
	PatchAbsCall(reinterpret_cast<size_t>(call_addr), reinterpret_cast<size_t>(new_function_ptr));
}

/*
	Patches an absolute call
*/
inline void PatchAbsCall(size_t call_addr, void* new_function_ptr)
{
	PatchAbsCall(call_addr, reinterpret_cast<size_t>(new_function_ptr));
}

/*
	Patches an absolute call
*/
inline void PatchAbsCall(void* call_addr, size_t new_function_ptr)
{
	PatchAbsCall(reinterpret_cast<size_t>(call_addr), new_function_ptr);
}

template<typename T>
inline T ReadFromAddress(size_t address)
{
	T* ptr = reinterpret_cast<T*>(address);
	return *ptr;
}