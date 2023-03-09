// levstring.h: interface for the levstring class.
//
//////////////////////////////////////////////////////////////////////

/************************************/
/* 19/09/2013: Support for WinCE    */
/************************************/

/************************************/
/* 02/01/2012: use of SafeList      */
/************************************/

/************************************/
/* 25/01/2011: Added thousands      */
/* separator char support.          */
/************************************/

/************************************/
/* 15/11/2010: Added full support   */
/* for wchar_t.                     */
/************************************/

/************************************/
/* 15/07/2010: Added support for    */
/* wchar_t return. Implemented      */
/* (wchar_t*) operator              */
/************************************/

#if !defined(AFX_LEVSTRING_H__856E81EE_17BC_4AE5_A8AC_A80A3E5C6D1B__INCLUDED_)
#define AFX_LEVSTRING_H__856E81EE_17BC_4AE5_A8AC_A80A3E5C6D1B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000


#include "Safelist.h"
//#include "iostream"
//#include "fstream"

#define STRING_VER "3.1"

//template <class T> class Lista; //por la referencia circular

namespace lev
	{
	class string 
		{
		private:
			char*			mString;
			wchar_t*		mWString;
			unsigned long	mLongitud;
			int				mPrecisionDecimales;
	
			void GenerarString(const char* string_, unsigned long largo_ = -1);
			void GenerarString(const wchar_t* string_, unsigned long largo_ = -1);
			void GenerarStringDecimal(const double val_, char thousandSeparator_ = 0, char decimalSeparator_ = 0);



		public:
			string();
			string(const string& string_);
			string(const char* string_, long largo_ = -1);
			string(const wchar_t* string_, long largo_ = -1);
  			string(const char val_);
  			string(const short int val_);
			string(const int val_);
			string(const long val_);
			string(const unsigned char val_);
			string(const unsigned short int val_);
			string(const unsigned int val_);
			string(const unsigned long val_);
			string(const float val_);
			string(const double val_);
			string(const float val_, int precision_, char thousandSeparator_ = 0, char decimalSeparator_ = 0);
			string(const double val_, int precision_, char thousandSeparator_ = 0, char decimalSeparator_ = 0);
			
			virtual ~string();

			int& PrecisionDecimal(void);
			int  PrecisionDecimal(void)const;

			unsigned long Longitud(void);
			void Borrar(void);

			SafeList<string> Split(char delimitador_ = ' ');

			string& ToLower(void);
			string& ToUpper(void);

			string& Trim(void);
			string& TrimLeft(void);
			string& TrimRight(void);

			string Left(unsigned long cantidad_, char charPad_ = 0);
			string Right(unsigned long cantidad_, char charPad_ = 0);
			string Mid(unsigned long inicio_, unsigned long cantidad_);

			bool EsNumero(void);
			bool EsNumeroEntero(void);
			bool EsNumeroNatural(void);

			string& operator += (const string& string_);
			string& operator += (const char* string_);
			string& operator += (const wchar_t* string_);
			string& operator += (const char string_);
  			string& operator += (const short int val_);
			string& operator += (const int val_);
			string& operator += (const long val_);
			string& operator += (const unsigned char val_);
			string& operator += (const unsigned short int val_);
			string& operator += (const unsigned int val_);
			string& operator += (const unsigned long val_);
			string& operator += (const float val_);
			string& operator += (const double val_);

			string operator + (const string& string_);
			string operator + (const char* string_);
			string operator + (const wchar_t* string_);
			string operator + (const char string_);
 			string operator + (const short int val_);
			string operator + (const int val_);
			string operator + (const long val_);
			string operator + (const unsigned char val_);
			string operator + (const unsigned short int val_);
			string operator + (const unsigned int val_);
			string operator + (const unsigned long val_);
			string operator + (const float val_);
			string operator + (const double val_);

			friend string operator + (const char* string_, const string& string2_);
			friend string operator + (const wchar_t* string_, const string& string2_);
			friend string operator + (const char string_, const string& string2_);
 			friend string operator + (const short int val_, const string& string2_);
			friend string operator + (const int val_, const string& string2_);
			friend string operator + (const long val_, const string& string2_);
			friend string operator + (const unsigned char val_, const string& string2_);
			friend string operator + (const unsigned short int val_, const string& string2_);
			friend string operator + (const unsigned int val_, const string& string2_);
			friend string operator + (const unsigned long val_, const string& string2_);
			friend string operator + (const float val_, const string& string2_);
			friend string operator + (const double val_, const string& string2_);

			string& operator = (const string& string_);
			string& operator = (const char* string_);
			string& operator = (const wchar_t* string_);
			string& operator = (const char val_);
 			string& operator = (const short int val_);
			string& operator = (const int val_);
			string& operator = (const long val_);
			string& operator = (const unsigned char val_);
			string& operator = (const unsigned short int val_);
			string& operator = (const unsigned int val_);
			string& operator = (const unsigned long val_);
			string& operator = (const float val_);
			string& operator = (const double val_);
			
			bool operator == (const string& string_);
			bool operator == (const char* string_);
			bool operator == (const wchar_t* string_);

			bool operator != (const string& string_);
			bool operator != (const char* string_);
			bool operator != (const wchar_t* string_);

			char operator [] (const long indice) const;
			char operator [] (const int indice) const;
			char operator [] (const unsigned long indice) const;
			char operator [] (const unsigned int indice) const;

			string& operator >> (string& res_);
			string& operator >> (char* res_);
			string& operator >> (wchar_t* res_);
			string& operator >> (char& res_);
			string& operator >> (short int& res_);
			string& operator >> (int& res_);
			string& operator >> (long& res_);
			string& operator >> (unsigned char& res_);
			string& operator >> (unsigned short int& res_);
			string& operator >> (unsigned int& res_);
			string& operator >> (unsigned long& res_);
			string& operator >> (float& res_);
			string& operator >> (double& res_);

			//string& operator << (std::istream& stream_);
			string& operator << (string val_);
			string& operator << (char val_);
			string& operator << (short int val_);
			string& operator << (int val_);
			string& operator << (long val_);
			string& operator << (unsigned char val_);
			string& operator << (unsigned short int val_);
			string& operator << (unsigned int val_);
			string& operator << (unsigned long val_);
			string& operator << (float val_);
			string& operator << (double val_);

			operator const char *() const;
			operator char *() const;

			operator const wchar_t* ();
			operator wchar_t* ();

			inline const char* Version(void) {return STRING_VER;}
		};
	};



#endif // !defined(AFX_LEVSTRING_H__856E81EE_17BC_4AE5_A8AC_A80A3E5C6D1B__INCLUDED_)
