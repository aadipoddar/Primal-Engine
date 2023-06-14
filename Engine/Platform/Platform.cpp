#include "Platform.h"
#include "PlatformTypes.h"

namespace primal::platform {

#ifdef _WIN64

	namespace {

		struct window_info
		{
			HWND	hwnd { nullptr };
			RECT	client_area { 0, 0, 1920, 1080 };
			RECT	fullscreen_area {};
			POINT	top_left { 0, 0 };
			DWORD	style { WS_VISIBLE };
			bool	is_fullscreen { false };
			bool	is_closed { false };
		};

		LRESULT CALLBACK internal_window_proc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
		{
			LONG_PTR long_ptr { GetWindowLongPtr(hwnd, 0) };

			return long_ptr
				? ((window_proc)long_ptr)(hwnd, msg, wParam, lParam)
				: DefWindowProc(hwnd, msg, wParam, lParam);
		}

	} // anonymous namespace


	window create_window(const window_init_info* const init_info /* = nullptr */)
	{
		window_proc callback { init_info ? init_info->callback : nullptr };
		window_handle parent { init_info ? init_info->parent : nullptr };

		// Setup a window Class
		WNDCLASSEX wc;
		ZeroMemory(&wc, sizeof(wc));
		wc.cbSize = sizeof(WNDCLASSEX);
		wc.style = CS_HREDRAW | CS_VREDRAW;
		wc.lpfnWndProc = internal_window_proc;
		wc.cbClsExtra = 0;
		wc.cbWndExtra = callback ? sizeof(callback) : 0;
		wc.hInstance = 0;
		wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
		wc.hCursor = LoadCursor(NULL, IDC_ARROW);
		wc.hbrBackground = CreateSolidBrush(RGB(26, 48, 76));
		wc.lpszMenuName = NULL;
		wc.lpszClassName = L"PrimalWindow";
		wc.hIconSm = LoadIcon(NULL, IDI_APPLICATION);

		// Register the window Class
		RegisterClassEx(&wc);

		window_info info {};
		RECT rc { info.client_area };

		// Adjust the window size for correct device size
		AdjustWindowRect(&rc, info.style, FALSE);

		const wchar_t* caption { (init_info && init_info->caption) ? init_info->caption : L"Primal Game" };

		const s32 left { (init_info && init_info->left) ? init_info->left : info.client_area.left };
		const s32 top { (init_info && init_info->top) ? init_info->top : info.client_area.top };

		const s32 width { (init_info && init_info->width) ? init_info->width : rc.right - rc.left };
		const s32 height { (init_info && init_info->height) ? init_info->height : rc.bottom - rc.top };

		info.style |= parent ? WS_CHILD : WS_OVERLAPPEDWINDOW;

		// Create an instanc of the window class
		info.hwnd = CreateWindowEx(
			0,					// Extended Style
			wc.lpszClassName,	// Window Class Name
			caption,			// Instance Title
			info.style,			// Window Style
			left, top,			// Initial Window Position
			width, height,		// Initial Window dimensions
			parent,				// Handle to Parent Window
			NULL,				// Handle to Menu
			NULL,				// Instance of this Application
			NULL				// Extra Creation Parameters
		);

		if (info.hwnd)
		{

		}

		return {};
	}

#elif
#error "Must Implement at least one platform"

#endif // _WIN64

}