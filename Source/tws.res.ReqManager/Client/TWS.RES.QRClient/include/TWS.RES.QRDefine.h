#ifndef __TWS_RES_MPQMDEFINE_H
#define __TWS_RES_MPQMDEFINE_H

#define CFG_DEBUG_MODE "DEBUG_MODE"

#include "string.h"

#ifdef BYTE_ALIGN
	#undef BYTE_ALIGN
#endif

#if defined(UNDER_CE)
	#define BYTE_ALIGN

	#ifndef max
	#define max(a,b)            (((a) > (b)) ? (a) : (b))
	#endif

	#ifndef min
	#define min(a,b)            (((a) < (b)) ? (a) : (b))
	#endif

#else
	#define BYTE_ALIGN __declspec(align(1))
#endif

namespace QRPROMOTION_MSG
{
	const __int32 VALIDATE = 0x01;
	const __int32 REDEEM   = 0x02;
};

#endif