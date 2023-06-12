#include "Common.h"
#include "CommonHeaders.h"

#ifndef WIN32_MEAN_AND_LEAN
#define WIN32_MEAN_AND_LEAN
#endif // !WIN32_MEAN_AND_LEAN


#include <Windows.h>

using namespace primal;


namespace {
	HMODULE game_code_dll { nullptr };
} // anonumous namespace


EDITOR_INTERFACE u32 
LoadGameCodeDLL(const char* dll_path)
{
	if (game_code_dll) return false;
	game_code_dll = LoadLibraryA(dll_path);
	assert(game_code_dll);

	return game_code_dll ? TRUE : FALSE;
}

EDITOR_INTERFACE u32
UnloadGameCodeDLL(const char* dll_path)
{
	if (!game_code_dll)	return FALSE;
	assert(game_code_dll);
	int result { FreeLibrary(game_code_dll) };
	assert(result);
	game_code_dll = nullptr;
	return TRUE;
}