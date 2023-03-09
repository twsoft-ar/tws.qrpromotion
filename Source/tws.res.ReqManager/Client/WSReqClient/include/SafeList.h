#pragma once

#ifdef _MFC_ 
	#include <afxwin.h>
#else
	#include <windows.h>
#endif

#include <list>

//para compatibilidad con compiladores posteriores a VS6
#if (_MSC_VER > 1300) && !defined(UNDER_CE) && !defined(WINCE)
#define InterlockedCompareExchange InterlockedCompareExchangePointer
#endif

namespace lev
{
	namespace LISTSTATE
	{
		const int UNLOCKED	= 0;
		const int LOCKED	= 1;
	};

	template <class T> class SafeList
	{
	private:
		std::list<T> mList; 
		T mTopElement;				

		unsigned long mLocked, mUnlocked;
		
	#if !defined(UNDER_CE) && !defined(WINCE)
		unsigned long *mLockingState;

		void Lock(void)
		{
			while(*(DWORD*)InterlockedCompareExchange((PVOID*) &mLockingState, &mLocked, &mUnlocked) == mLocked)
				Sleep(10); 
		}

		void Unlock(void)
		{
			mLockingState = &mUnlocked;
		}
	#else
		unsigned long mLockingState;

		void Lock(void)
		{
			while(InterlockedCompareExchange((LPLONG)&mLockingState, mLocked, mUnlocked) == mLocked) 
				Sleep(10);
		}

		void Unlock(void)
		{
			mLockingState = mUnlocked;
		}
	#endif
	
	public:

		SafeList(void)
		:	mLocked(lev::LISTSTATE::LOCKED), 
			mUnlocked(lev::LISTSTATE::UNLOCKED)
		{
		#if !defined(UNDER_CE) && !defined(WINCE)
			mLockingState = &mUnlocked;
		#else
			mLockingState = mUnlocked;
		#endif
		}

		~SafeList(void) 
		{}

		void Add(const T& elem_)
		{
			try
			{
				Lock();
				mList.push_back(elem_);
			}
			catch(...)
			{
			}

			Unlock();
		}

		void Remove(unsigned long index_)
		{
			if(mList.size() > index_)
			{
				std::list<T>::iterator iter = mList.begin();
					
				for(unsigned long i=0 ; i < index_; i++)
					iter++;
				
				mList.erase(iter);
			}
		}

		T Get(unsigned long index_) const
		{
			T res;
			try
			{
				//Lock();
				const_cast<SafeList<T>*>(this)->Lock();

				if(mList.size() > index_)
				{
					std::list<T>::const_iterator iter = mList.begin();
					
					for(unsigned long i=0 ; i < index_; i++)
						iter++;
					
					res = *iter;
				}
			}
			catch(...)
			{
			}

			const_cast<SafeList<T>*>(this)->Unlock();
			//Unlock();

			return res;
		}

		void Set(unsigned long index_, T value_)
		{
			try
			{
				Lock();
				if(mList.size() > index_)
				{
					std::list<T>::iterator iter = mList.begin();
					
					for(int i=0; i<index_; i++)
						iter++;			
					
					*iter = value_;
					Unlock();
				}
			}
			catch(...)
			{
				Unlock();
			}
		}


		bool Empty(void)
		{
			return mList.empty();
		}
		
		void Clear(void)
		{
			try
			{
				Lock();				
				mList.clear();
				Unlock();				
			}
			catch(...)
			{
				Unlock();
				throw("Empty() Exception: trying to clear list");
			}
		}
		
		unsigned long Size(void)
		{
			return mList.size();
		}

		T& operator [] (unsigned long index_)
		{
			try
			{
				Lock();
				if(mList.size() > index_)
				{
					std::list<T>::iterator iter = mList.begin();
					
					for(int i=0; i<index_; i++)
						iter++;			
					
					Unlock();
					return *iter;
				}
				else
				{
					Unlock();
					throw (lev::string("T& operator [] Exception: index out of range"));
				}
			}
			catch(...)
			{
				Unlock();
				throw (lev::string("T& operator [] Exception: undefined"));
			}
		}

		T operator [] (unsigned long index_) const
		{
			if(mList.size() > index_)
			{
				return Get(index_);
			}
			else
				throw (lev::string("T& operator [] Exception: index out of range"));
		}

	};
};