#include "D3D12Upload.h"
#include "D3D12Core.h"

namespace primal::graphics::d3d12::upload {

	namespace {

		struct upload_frame
		{
			ID3D12CommandAllocator* cmd_allocator{ nullptr };
			id3d12_graphics_command_list* cmd_list{ nullptr };
			ID3D12Resource* upload_buffer{ nullptr };
			void* cpu_address{ nullptr };
			u64 fence_value{ 0 };

			void wait_and_reset();

			void release()
			{
				wait_and_reset();
				core::release(cmd_allocator);
				core::release(cmd_list);
			}

			constexpr bool is_ready() const { return upload_buffer == nullptr; }
		};

		constexpr u32 upload_frame_count{ 4 };
		upload_frame upload_frames[upload_frame_count]{};
		ID3D12CommandQueue* upload_cmd_queue{ nullptr };
		ID3D12Fence1* upload_fence{ nullptr };
		u64 upload_fence_value{ 0 };
		HANDLE fence_event{};

		void upload_frame::wait_and_reset()
		{
			assert(upload_fence && fence_event);
			if (upload_fence->GetCompletedValue() < fence_value)
			{
				DXCall(upload_fence->SetEventOnCompletion(fence_value, fence_event));
				WaitForSingleObject(fence_event, INFINITE);
			}

			core::release(upload_buffer);
			cpu_address = nullptr;
		}

		bool init_failed()
		{
			shutdown();
			return false;
		}

	} // Anonymouse Namespace

	bool initialize()
	{
		id3d12_device* const device{ core::device() };
		assert(device && !upload_cmd_queue);

		HRESULT hr{ S_OK };

		for (u32 i{ 0 }; i < upload_frame_count; ++i)
		{
			upload_frame& frame{ upload_frames[i] };
			DXCall(hr = device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_COPY, IID_PPV_ARGS(&frame.cmd_allocator)));
			if (FAILED(hr)) return init_failed();


			DXCall(hr = device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_COPY, frame.cmd_allocator, nullptr, IID_PPV_ARGS(&frame.cmd_list)));
			if (FAILED(hr)) return init_failed();

			DXCall(frame.cmd_list->Close());

			NAME_D3D12_OBJECT_INDEXED(frame.cmd_allocator, i, L"Upload Command Allocator");
			NAME_D3D12_OBJECT_INDEXED(frame.cmd_list, i, L"Upload Command List");
		}

		D3D12_COMMAND_QUEUE_DESC desc{};
		desc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
		desc.NodeMask = 0;
		desc.Priority = D3D12_COMMAND_QUEUE_PRIORITY_NORMAL;
		desc.Type = D3D12_COMMAND_LIST_TYPE_COPY;

		DXCall(hr = device->CreateCommandQueue(&desc, IID_PPV_ARGS(&upload_cmd_queue)));
		if (FAILED(hr)) return init_failed();
		NAME_D3D12_OBJECT(upload_cmd_queue, L"Upload Copy Queue");

		DXCall(hr = device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&upload_fence)));
		if (FAILED(hr)) return init_failed();
		NAME_D3D12_OBJECT(upload_fence, L"Upload Fence");

		fence_event = CreateEventEx(nullptr, nullptr, 0, EVENT_ALL_ACCESS);
		assert(fence_event);
		if (!fence_event) return init_failed();

		return true;
	}

	void shutdown()
	{
		for (u32 i{ 0 }; i < upload_frame_count; ++i)
		{
			upload_frames[i].release();
		}

		if (fence_event)
		{
			CloseHandle(fence_event);
			fence_event = nullptr;
		}

		core::release(upload_cmd_queue);
		core::release(upload_fence);
		upload_fence_value = 0;
	}

}