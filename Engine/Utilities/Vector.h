#pragma once
#include "CommonHeaders.h"

namespace primal::utl {

	// A Vector classs similar to std::vector withh basic functionality.
	// The user can specify in the template argument whether they want
	// elements' destructor to be called when being removed or while
	// clearing/destructing the vector
	template<typename T, bool destruct = true>
	class vector
	{
	public:
		//Default constructor. Doesn't allocate memory
		vector() = default;

		// Constructor resizes the vector and initializes 'count' items.
		constexpr explicit vector(u64 count)
		{
			resize(count);
		}

		// Constructor resizes the vector and initializes 'count' items using 'value'.
		constexpr explicit vector(u64 count, const T& value)
		{
			resize(count, value);
		}

		// Copy-constructor. Constructs by copying another vector.
		// The items in the copied vector must be copyable.
		constexpr vector(const vector& o)
		{
			*this = o;
		}

		// Move-constructor. Constructs by moving another vector.
		// The original vector will be empty after move.
		constexpr vector(const vector&& o)
			:_capacity{ o._capacity }, _size{ o._size }, _data{ o._data }
		{
			o.reset();
		}

		// Copy - Assignment operator, Clears this vector and copies items
		// from another vector. The items must be copyable.
		constexpr vector& operator=(const vector& o)
		{
			assert(this != std::addressof(o));
			if (this != std::addressof(o))
			{
				clear();
				reserve(o._size);
				for (auto& item : o)
				{
					emplace_back(item);
				}
				assert(_size == o._size);
			}

			return *this;
		}

		// Move-Assignment operator. Frees all resources in this vector and
		// moves the other vector into this one.
		constexpr vector& operator=(const vector&& o)
		{
			assert(this != std::addressof(o));
			if (this != std::addressof(o))
			{
				destroy();
				move(o);
			}

			return *this;
		}

		~vector() { destroy(); }

		// Clears the vector and destructs items as specified in template argument.
		constexpr void clear()
		{
			if constexpr (destruct)
			{
				destruct_range(0, _size);
			}
			_size = 0;
		}

	private:

		constexpr void move(vector& o)
		{
			_capacity = o._capacity;
			_size = o._size;
			_data = o._data;
			o.reset();
		}

		constexpr void reset()
		{
			_capacity = 0;
			_size = 0;
			_data = nullptr;
		}

		constexpr void destruct_range(u64 first, u64 last)
		{
			assert(destruct);
			assert(first <= _size && last <= _size && first <= last);
			if (_data)
			{
				for (; first != last; ++first)
				{
					_data[first].~T();
				}
			}
		}

		constexpr void destroy()
		{
			assert([&] {return _capacity ? _data != nullptr : _data == nullptr; }());
			clear();
			_capacity = 0;
			if (_data) free(_data);
			_data = nullptr;
		}

		u64 _capacity{ 0 };
		u64 _size{ 0 };
		T* _data{ nullptr };
	};

}