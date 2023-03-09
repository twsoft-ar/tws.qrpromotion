// levstring.cpp: implementation of the levstring class.
//
//////////////////////////////////////////////////////////////////////
#include "levstring.h"
#include "string.h"

#if defined(UNDER_CE) || defined (WINCE)
#define itoa	_itoa
#define ltoa	_ltoa
#define ultoa	_ultoa
#define strlwr	_strlwr
#define strupr	_strupr
#define fcvt	_fcvt
#endif

namespace lev
{
string::string()
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	GenerarString("", 0);
	}

string::string(const string& string_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	GenerarString(string_.mString, string_.mLongitud);
	}

string::string(const char* string_, long largo_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	GenerarString(string_, largo_);
	}

string::string(const wchar_t* string_, long largo_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
{
	GenerarString(string_, largo_);
}

string::string(const char val_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	char aux[2];

	aux[0] =  val_;
	aux[1] = '\0'; 

	GenerarString(aux);
	}

string::string(const short int val_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	char aux[64];
	GenerarString(itoa(val_, aux, 10));
	}

string::string(const int val_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	char aux[64];
	GenerarString(itoa(val_, aux, 10));
	}

string::string(const long val_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	char aux[64];
	GenerarString(ltoa(val_, aux, 10));
	}

string::string(const unsigned char val_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	char aux[2];

	aux[0] =  val_;
	aux[1] = '\0'; 

	GenerarString(aux);
	}

string::string(const unsigned short int val_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	char aux[64];
	GenerarString(ultoa(val_, aux, 10));
	}

string::string(const unsigned int val_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	char aux[64];
	GenerarString(ultoa(val_, aux, 10));
	}

string::string(const unsigned long val_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	char aux[64];
	GenerarString(ultoa(val_, aux, 10));
	}

string::string(const float val_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	GenerarStringDecimal(val_);
	}

string::string(const double val_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(5), mWString(NULL)
	{
	GenerarStringDecimal(val_);
	}

string::string(const float val_, int precision_, char thousandSeparator_, char decimalSeparator_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(precision_), mWString(NULL)
	{
	GenerarStringDecimal(val_, thousandSeparator_, decimalSeparator_);
	}

string::string(const double val_, int precision_, char thousandSeparator_, char decimalSeparator_)
:	mString(NULL), mLongitud(0), mPrecisionDecimales(precision_), mWString(NULL)
	{
	GenerarStringDecimal(val_, thousandSeparator_, decimalSeparator_);
	}

string::~string()
	{
	Borrar();
	}

int& string::PrecisionDecimal(void)
	{
	return mPrecisionDecimales;
	}


int string::PrecisionDecimal(void)const
	{
	return mPrecisionDecimales;
	}

unsigned long string::Longitud(void)
	{
	return mLongitud;
	}

void string::Borrar(void)
	{
	if(mString)
		delete[] mString;

	mString = NULL;
	mLongitud = 0;

	if(mWString)
		delete[] mWString;

	mWString = NULL;
	}

SafeList<string> string::Split(char delimitador_)
	{
	SafeList<string> res;
	unsigned long indice, segmentos = 0;
	string aux;

	//parseo el string (incluido el null char)
	for(indice = 0; indice <= mLongitud; indice++)
		{
		if(mString[indice] == '\0' || mString[indice] == delimitador_)
			{
			res.Add(aux);
			aux.Borrar();
			}
		else //voy agregando los caracteres
			aux += mString[indice];
		}

	return res;
	}

string& string::ToLower(void)
	{
	if(mLongitud > 0)
		strlwr(mString);
	return *this;
	}

string& string::ToUpper(void)
	{
	if(mLongitud > 0)
		strupr(mString);
	return *this;
	}

string& string::Trim(void)
	{
	return TrimLeft().TrimRight();
	}

string& string::TrimLeft(void)
	{
	unsigned long indice = 0, nuevaLongitud;
	
	//recorro hasta que encuentro el primer caracter distinto de espacio 	
	while(indice < mLongitud && (mString[indice] == ' ' || mString[indice] == '\t' || mString[indice] == '\r'|| mString[indice] == '\n'))
		indice++;

	nuevaLongitud = mLongitud - indice;
	
	string stringAux(mString+indice, nuevaLongitud);
	GenerarString(stringAux.mString, stringAux.mLongitud);

	return *this;
	}

string& string::TrimRight(void)
	{
	long indice = mLongitud;
	
	//recorro hasta que encuentro el primer caracter distinto de espacio 	4
	while(indice > 0 && (mString[indice-1] == ' ' || mString[indice-1] == '\t' || mString[indice-1] == '\r' || mString[indice-1] == '\n'))
		indice--;
	
	string stringAux(mString, indice);
	GenerarString(stringAux.mString, stringAux.mLongitud);

	return *this;
	}

string string::Left(unsigned long cantidad_, char charPad_)
	{
	unsigned long largoPad = cantidad_;
	char* auxStr = NULL; 

	if(cantidad_ > mLongitud)
		cantidad_ = mLongitud;

	string res(mString, cantidad_);
	
	//padding
	largoPad -= cantidad_;
	
	if(charPad_ != 0 && largoPad > 0)
		{
		if(auxStr = new char[largoPad+1])
			{
			for(unsigned long i=0 ; i < largoPad; i++)
				auxStr[i] = charPad_;

			auxStr[largoPad] = '\0';

			res += auxStr;
			delete[] auxStr;
			}
		}

	return res;
	}

string string::Right(unsigned long cantidad_, char charPad_)
	{
	unsigned long largoPad = cantidad_;
	char* auxStr = NULL; 

	if(cantidad_ > mLongitud)
		cantidad_ = mLongitud;

	string res(mString + (mLongitud - cantidad_), cantidad_);

	//padding
	largoPad -= cantidad_;
	
	if(charPad_ != 0 && largoPad > 0)
		{
		if(auxStr = new char[largoPad+1])
			{
			for(unsigned long i=0 ; i < largoPad; i++)
				auxStr[i] = charPad_;

			auxStr[largoPad] = '\0';

			res = string(auxStr) + res;
			delete[] auxStr;
			}
		}
	return res;
	}

string string::Mid(unsigned long inicio_, unsigned long cantidad_)
	{
	if(cantidad_ > mLongitud - inicio_)
		cantidad_ = mLongitud - inicio_;

	string res(mString + inicio_, cantidad_);

	return res;
	}

string& string::operator += (const string& string_)
	{
	unsigned long indice = 0;

	//copio solo si es necesario
	if(string_.mLongitud > 0)
		{
		string aux(*this);
	
		Borrar();

		mLongitud = aux.mLongitud + string_.mLongitud;
		mString = new char[mLongitud + 1];

		for(indice = 0; indice < aux.mLongitud; indice++)
			mString[indice] = aux.mString[indice];

		for(indice = aux.mLongitud; indice < string_.mLongitud + aux.mLongitud; indice++)
			mString[indice] = string_.mString[indice - aux.mLongitud];
		
		mString[indice] = '\0';
		}

	return *this;
	}

string& string::operator += (const char* string_)
	{
	string aux(string_);
	(*this) += aux;

	return *this;
	}

string& string::operator += (const wchar_t* string_)
{
	string aux(string_);
	(*this) += aux;

	return *this;
}

string& string::operator += (const char string_)
	{
	string aux(string_);
	(*this) += aux;

	return *this;
	}

string& string::operator += (const short int val_)
	{
	string aux(val_);
	(*this) += aux;

	return *this;
	}

string& string::operator += (const int val_)
	{
	string aux(val_);
	(*this) += aux;

	return *this;
	}

string& string::operator += (const long val_)
	{
	string aux(val_);
	(*this) += aux;

	return *this;
	}

string& string::operator += (const unsigned char val_)
	{
	string aux(val_);
	(*this) += aux;

	return *this;
	}

string& string::operator += (const unsigned short int val_)
	{
	string aux(val_);
	(*this) += aux;

	return *this;
	}

string& string::operator += (const unsigned int val_)
	{
	string aux(val_);
	(*this) += aux;

	return *this;
	}

string& string::operator += (const unsigned long val_)
	{
	string aux(val_);
	(*this) += aux;

	return *this;
	}

string& string::operator += (const float val_)
	{
	string aux;
	aux.mPrecisionDecimales = mPrecisionDecimales;
	aux.GenerarStringDecimal(val_);

	(*this) += aux;

	return *this;
	}

string& string::operator += (const double val_)
	{
	string aux;
	aux.mPrecisionDecimales = mPrecisionDecimales;
	aux.GenerarStringDecimal(val_);

	(*this) += aux;

	return *this;
	}

string string::operator + (const string& string_)
	{
	string res(*this);
	res += string_;
	return  res;
	}

string string::operator + (const char* string_)
	{
	string aux(string_);
	return (*this) + aux;
	}

string string::operator + (const wchar_t* string_)
{
	string aux(string_);
	return (*this) + aux;
}

string string::operator + (const char string_)
	{
	string aux(string_);
	return (*this) + aux;
	}

string string::operator + (const short int val_)
	{
	string aux(val_);
	return (*this) + aux;
	}

string string::operator + (const int val_)
	{
	string aux(val_);
	return (*this) + aux;
	}

string string::operator + (const long val_)
	{
	string aux(val_);
	return (*this) + aux;
	}

string string::operator + (const unsigned char val_)
	{
	string aux(val_);
	return (*this) + aux;
	}

string string::operator + (const unsigned short int val_)
	{
	string aux(val_);
	return (*this) + aux;
	}

string string::operator + (const unsigned int val_)
	{
	string aux(val_);
	return (*this) + aux;
	}

string string::operator + (const unsigned long val_)
	{
	string aux(val_);
	return (*this) + aux;
	}

string string::operator + (const float val_)
	{
	string aux;
	aux.mPrecisionDecimales = mPrecisionDecimales;
	aux.GenerarStringDecimal(val_);

	return (*this) + aux;
	}

string string::operator + (const double val_)
	{
	string aux;
	aux.mPrecisionDecimales = mPrecisionDecimales;
	aux.GenerarStringDecimal(val_);

	return (*this) + aux;
	}

string operator + (const char* string_, const string& string2_)
{
	string res(string_);
	return  res + string2_;
}

string operator + (const wchar_t* string_, const string& string2_)
{
	string res(string_);
	return  res + string2_;
}
string operator + (const char string_, const string& string2_)
{
	string res(string_);
	return  res+string2_;
}

string operator + (const short int val_, const string& string2_)
{
	string res(val_);
	return  res+string2_;
}

string operator + (const int val_, const string& string2_)
{
	string res(val_);
	return  res+string2_;
}

string operator + (const long val_, const string& string2_)
{
	string res(val_);
	return  res+string2_;
}

string operator + (const unsigned char val_, const string& string2_)
{
	string res(val_);
	return  res+string2_;
}

string operator + (const unsigned short int val_, const string& string2_)
{
	string res(val_);
	return  res+string2_;
}

string operator + (const unsigned int val_, const string& string2_)
{
	string res(val_);
	return  res+string2_;
}

string operator + (const unsigned long val_, const string& string2_)
{
	string res(val_);
	return  res+string2_;
}

string operator + (const float val_, const string& string2_)
{
	string res(val_);
	return  res+string2_;
}

string operator + (const double val_, const string& string2_)
{
	string res(val_);
	return  res+string2_;
}

string& string::operator = (const string& string_)
	{
	GenerarString(string_.mString, string_.mLongitud);
	return *this;
	}

string& string::operator = (const char* string_)
	{
	string aux(string_);
	(*this) = aux;

	return *this;
	}

string& string::operator = (const wchar_t* string_)
{
	string aux(string_);
	(*this) = aux;

	return *this;
}

string& string::operator = (const char val_)
	{
	string aux(val_);
	(*this) = aux;

	return *this;
	}

string& string::operator = (const short int val_)
	{
	string aux(val_);
	(*this) = aux;

	return *this;
	}

string& string::operator = (const int val_)
	{
	string aux(val_);
	(*this) = aux;

	return *this;
	}

string& string::operator = (const long val_)
	{
	string aux(val_);
	(*this) = aux;

	return *this;
	}

string& string::operator = (const unsigned char val_)
	{
	string aux(val_);
	(*this) = aux;

	return *this;
	}

string& string::operator = (const unsigned short int val_)
	{
	string aux(val_);
	(*this) = aux;

	return *this;
	}

string& string::operator = (const unsigned int val_)
	{
	string aux(val_);
	(*this) = aux;

	return *this;
	}

string& string::operator = (const unsigned long val_)
	{
	string aux(val_);
	(*this) = aux;

	return *this;
	}

string& string::operator = (const float val_)
	{
	string aux;
	aux.mPrecisionDecimales = mPrecisionDecimales;
	aux.GenerarStringDecimal(val_);

	(*this) = aux;

	return *this;
	}

string& string::operator = (const double val_)
	{
	string aux;
	aux.mPrecisionDecimales = mPrecisionDecimales;
	aux.GenerarStringDecimal(val_);

	(*this) = aux;

	return *this;
	}

bool string::operator == (const string& string_)
	{
	bool res;

	res = (mLongitud == string_.mLongitud);
	if(res)
		res = (memcmp(mString, string_.mString, mLongitud) == 0);

	return res;
	}

bool string::operator == (const char* string_)
	{
	return ((*this) == string(string_));
	}

bool string::operator == (const wchar_t* string_)
{
	return ((*this) == string(string_));
}

bool string::operator != (const string& string_)
	{
	return !(operator==(string_));
	}


bool string::operator != (const char* string_)
	{
	return !(operator==(string_));
	}

bool string::operator != (const wchar_t* string_)
{
	return !(operator==(string_));
}

char string::operator [] (const unsigned long indice) const
	{
	char res = 0;

	if(indice < mLongitud)
		res = mString[indice];

	return res;
	}

char string::operator [] (const long indice) const
	{
	char res = 0;

	if((unsigned long)indice < mLongitud)
		res = mString[indice];

	return res;
	}

char string::operator [] (const int indice) const
	{
	char res = 0;

	if((unsigned long)indice < mLongitud)
		res = mString[indice];

	return res;
	}

char string::operator [] (const unsigned int indice) const
	{
	char res = 0;

	if((unsigned long)indice < mLongitud)
		res = mString[indice];

	return res;
	}

//para los operadores de extraccion uso como delimitadores ' ', '\t', '\n', '\r', 
string& string::operator >> (string& res_)
	{
	string aux;
	unsigned long indice = 0;
	
	//avanzo al principio de primer string
	while(indice < mLongitud && (mString[indice] == ' ' || mString[indice] == '\t' || mString[indice] == '\n' || mString[indice] == '\r'))
		indice++;
	
	//lei el primer string
	while(indice < mLongitud && !(mString[indice] == ' ' || mString[indice] == '\t' || mString[indice] == '\n' || mString[indice] == '\r'))
		{
		aux += mString[indice];
		indice++;
		}

	res_ = aux;
	
	*this = Mid(indice, mLongitud); 

	return *this;
	}

string& string::operator >> (char* res_)
{
	if(res_)
		memcpy(res_, mString, mLongitud+1);

	return *this;
}

string& string::operator >> (wchar_t* res_)
{
	if(res_)
		wcscpy(res_, this->operator const wchar_t *()); 

	return *this;
}

string& string::operator >> (char& res_)
	{
	string aux;

	(*this) >> aux;
	res_ = (char)atoi(aux.mString);

	return *this;
	}

string& string::operator >> (short int& res_)
	{
	string aux;

	(*this) >> aux;
	res_ = (short int)atoi(aux.mString);

	return *this;
	}

string& string::operator >> (int& res_)
	{
	string aux;

	(*this) >> aux;
	res_ = atoi(aux.mString);

	return *this;
	}

string& string::operator >> (long& res_)
	{
	string aux;

	(*this) >> aux;
	res_ = atol(aux.mString);
	
	return *this;
	}

string& string::operator >> (unsigned char& res_)
	{
	string aux;

	(*this) >> aux;
	res_ = (unsigned char)atoi(aux.mString);
	
	return *this;
	}

string& string::operator >> (unsigned short int& res_)
	{
	string aux;

	(*this) >> aux;
	res_ = (unsigned short int)atoi(aux.mString);
	
	return *this;
	}

string& string::operator >> (unsigned int& res_)
	{
	string aux;

	(*this) >> aux;
	res_ = (unsigned int)atoi(aux.mString);
	
	return *this;
	}

string& string::operator >> (unsigned long& res_)
	{
	string aux;

	(*this) >> aux;
	res_ = (unsigned long) atol(aux.mString);
	
	return *this;
	}

string& string::operator >> (float& res_)
	{
	string aux;

	(*this) >> aux;
	res_ = (float) atof(aux.mString);
	
	return *this;
	}

string& string::operator >> (double& res_)
	{
	string aux;

	(*this) >> aux;
	res_ = atof(aux.mString);

	return *this;
	}

/*string& string::operator << (std::istream& stream_)
{
	char currChar = 0;

	while(stream_.read(&currChar, 1) && !stream_.eof() && (currChar == '\n'));

	while(!stream_.eof() && !(currChar == '\n'))
	{
		operator <<(currChar);
		stream_.read(&currChar, 1);
	}

	return *this;
}*/

string& string::operator << (string val_)
	{
	string aux(val_);
	return operator +=(aux);
	}

string& string::operator << (char val_)
	{
	string aux(val_);
	return operator <<(aux);
	}

string& string::operator << (short int val_)
	{
	string aux(val_);
	return operator <<(aux);
	}

string& string::operator << (int val_)
	{
	string aux(val_);
	return operator <<(aux);
	}

string& string::operator << (long val_)
	{
	string aux(val_);
	return operator <<(aux);
	}

string& string::operator << (unsigned char val_)
	{
	string aux(val_);
	return operator <<(aux);
	}

string& string::operator << (unsigned short int val_)
	{
	string aux(val_);
	return operator <<(aux);
	}

string& string::operator << (unsigned int val_)
	{
	string aux(val_);
	return operator <<(aux);
	}

string& string::operator << (unsigned long val_)
	{
	string aux(val_);
	return operator <<(aux);
	}

string& string::operator << (float val_)
	{
	string aux;
	aux.mPrecisionDecimales = mPrecisionDecimales;
	aux.GenerarStringDecimal(val_);

	return operator <<(aux);
	}

string& string::operator << (double val_)
	{
	string aux;
	aux.mPrecisionDecimales = mPrecisionDecimales;
	aux.GenerarStringDecimal(val_);

	return operator <<(aux);
	}

string::operator const char *() const
	{
	return mString;
	}

string::operator char *() const
	{
	return mString;
	}


string::operator const wchar_t *()
	{
	if(mWString)
		delete[] mWString;

	mWString = new wchar_t[mLongitud + 1];

	mbstowcs(mWString, mString, mLongitud);
	mWString[mLongitud] = 0;

	return mWString;
	}

string::operator wchar_t *()
	{
	if(mWString)
		delete[] mWString;

	mWString = new wchar_t[mLongitud + 1];

	mbstowcs(mWString, mString, mLongitud);
	mWString[mLongitud] = 0;

	return mWString;
	}

bool string::EsNumero(void)
	{
	bool res;
	int cantPuntos = 0, cantSignos = 0;
	res = mLongitud > 0;
	
	for(unsigned long i=0; res && i<mLongitud; i++)
		{
		res = res && (('0' <= mString[i] && mString[i] <= '9') || 
					(mString[i] == '.' && mLongitud > i && ++cantPuntos < 2 ) || 
					(mString[0] == '-' && mLongitud > 1 && ++cantSignos < 2) || 
					(mString[0] == '+' && mLongitud > 1 && ++cantSignos < 2));
		}
	
	return res;
	}

bool string::EsNumeroEntero(void)
	{
	bool res;
	int cantSignos = 0;

	res = mLongitud > 0;
	
	for(unsigned long i=0; res && i<mLongitud; i++)
		{
		res = res && (('0' <= mString[i] && mString[i] <= '9') || 
					(mString[0] == '-' && mLongitud > 1 && ++cantSignos < 2) || 
					(mString[0] == '+' && mLongitud > 1 && ++cantSignos < 2));
		}

	return res;
	}

bool string::EsNumeroNatural(void)
	{
	bool res;
	int cantSignos = 0;
	res = mLongitud > 0;
	
	for(unsigned long i=0; res && i<mLongitud; i++)
		{
		res = res && (('0' <= mString[i] && mString[i] <= '9') || 
					(mString[0] == '+' && mLongitud > 1 && ++cantSignos < 2));
		}

	return res;
	}

void string::GenerarString(const char* string_, unsigned long largo_)
	{
	Borrar();

	if(string_)
		{
		mLongitud = (unsigned long)strlen(string_);

		if(largo_ < mLongitud && largo_ != -1)
			mLongitud = largo_;
		}

	mString = new char[mLongitud+1];
	memcpy(mString, string_, mLongitud);
	mString[mLongitud] = '\0';
	}

void string::GenerarString(const wchar_t* string_, unsigned long largo_)
	{
	Borrar();

	if(string_)
		{
		mLongitud = (unsigned long)wcslen(string_);

		if(largo_ < mLongitud && largo_ != -1)
			mLongitud = largo_;
		}

	mString = new char[mLongitud+1];
	wcstombs(mString, string_, mLongitud);
	mString[mLongitud] = '\0';
	}

void string::GenerarStringDecimal(const double val_, char thousandSeparator_, char decimalSeparator_)
	{
	char* aux;
	string pad, mantisa;

	int punto, signo;

	if(decimalSeparator_ == 0)
		decimalSeparator_ = '.';

	if(decimalSeparator_ == '.' && thousandSeparator_ == '.')
		thousandSeparator_ = ',';
	
	aux = fcvt(val_, mPrecisionDecimales, &punto, &signo);

	if(punto < 0)
		{
		for(unsigned long p=0; p<(unsigned long)(-punto); p++)
			pad += '0';

		punto = 0;
		mantisa = pad;
		}

	mantisa += aux;

	if(signo)
		*this += '-';

	if(strlen(aux) == 0)
		*this = '0';

	for(unsigned long i=0; i<mantisa.mLongitud; i++)
		{
		if(i == (unsigned)punto && mPrecisionDecimales > 0)
			{
			if(punto == 0)
				*this += '0';

			//*this += '.';
			*this += decimalSeparator_;
			}

		*this += mantisa[i];

		if(punto - i > 3 && (punto - i) % 3 == 1 && thousandSeparator_ != 0)
			*this += thousandSeparator_;
		}
	}
} //end namespace lev

