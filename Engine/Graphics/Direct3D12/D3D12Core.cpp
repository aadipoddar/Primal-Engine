#include "D3D12Core.h"

namespace primal::graphics::d3d12::core {

	namespace {
		ID3D12Device8* main_device;
	} // anonymous namespace

	bool initialize()
	{
		// Determine which adapter (i.e. graphics card) to use
		// Determine what is the maximum feature level that is supported
		// Create a ID3D12Device (this is a virtual adapter)
	}

}