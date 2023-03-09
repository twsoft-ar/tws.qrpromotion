// Utils.h: interface for the Utils class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_UTILS_H__CA8B652E_15D0_4171_82DC_B008D0A33794__INCLUDED_)
#define AFX_UTILS_H__CA8B652E_15D0_4171_82DC_B008D0A33794__INCLUDED_

#include "levstring.h"
#include "ByteStream.h"
#include "string"
#include "fstream"

namespace UTILSSTATE
{
	const int UNLOCKED	= 0;
	const int LOCKED	= 1;
};

class Logger
{

private:

	
	static unsigned long mLocked, mUnlocked;
	static unsigned long *mLockingState;

	static void Lock(void)
	{
	#if !defined(UNDER_CE) && !defined(WINCE)
		while(*(DWORD*)InterlockedCompareExchange((PVOID*) &mLockingState, &mLocked, &mUnlocked) == mLocked) Sleep(10); 
	#else
		while(InterlockedCompareExchange((LPLONG)&mLockingState, mLocked, mUnlocked) == mLocked) Sleep(10); 
	#endif
	};

	static void Unlock(void)
	{
		mLockingState = &mUnlocked;
	}


public:
	Logger(void) {};
	~Logger(void) {};

	static lev::string GetTimestamp(bool getMs_ = true);
	static void LogEvent(lev::string fileName_, lev::string logMessage_, bool append_ = true);
};


#endif // !defined(AFX_UTILS_H__CA8B652E_15D0_4171_82DC_B008D0A33794__INCLUDED_)
