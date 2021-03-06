#include <../General/pch.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <math.h>
#include <sstream>
#include <General/strptime.h>
#include <General/date_class.h>
#include <General/stream_functions.h>
#include <General/string_functions.h>

#define NO_YEAR  4200
#define NO_YEAR_STRING "4200"

using namespace std;

static const char *Month_str[12] =
   {"January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"};
static const unsigned int Acc_days_in_month[13] =
   {0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365};

// *******************************************************************
GDate::GDate(void)  {
// *******************************************************************

//  Short description:
//    constructor

//  Notes:

//  Changes:
//    DPH 16/1/95

//  Calls:

//  Internal variables
//    none

// -------------------- Executable code section ----------------------

   Format_string = "D/M/YYYY";
   Must_have_year = false;
   Julian_day = 0;
   }

GDate::GDate(unsigned jd)
{
   Format_string = "D/M/YYYY";
   Must_have_year = false;
   Julian_day = jd;
}


// *******************************************************************
GDate::GDate(unsigned int Day, unsigned int Month, unsigned int Year)  {
// *******************************************************************

//  Short description:
//    constructor

//  Notes:

//  Changes:

//  Calls:

//  Internal variables
//    none

// -------------------- Executable code section ----------------------

   Format_string = "D/M/YYYY";
   Must_have_year = false;
   Julian_day = 0;
   Set(Day, Month, Year);
   }

// *******************************************************************
void GDate::Read(const char *Str)  {
// *******************************************************************

//  Short description:
//    Read a date (in normal format) from string

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables

// -------------------- Executable code section ----------------------

   istringstream In_stream((char*) Str);
   Read(In_stream);
   }

// ------------------------------------------------------------------
//  Short description:
//    convert a month string to an integer.  Returns 0 if not valid string.

//  Notes:
//    handles "mar" or "march" style months.

//  Changes:
//    DPH 6/9/1996
//    dph 27/3/98 changed NPOS to NPOS in line with standard.

// ------------------------------------------------------------------
int Month_string_2_integer (string& Month_string)
   {
   bool Found = false;
   int Month;
   for (Month = 0;
        Month < 12 && !Found;
        Month++)
      {
      string This_string;
      This_string = Month_str[Month];

      if (Month_string.length() == 3)
         This_string.replace (3, string::npos, "");

      Found = (Str_i_Eq(This_string, Month_string));
      }

   if (Found)
      return Month;

   else
      return 0;

   }

// *******************************************************************
       void GDate::Read(istream& In_stream)  {
// *******************************************************************

//  Short description:
//    Read a date (in normal format) from in stream

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables
      string Day_string;              // Day string
      string Month_string;            // Month string
      string Year_string;             // Year string
      unsigned int Day,Month,Year;     // day, month, year
      bool Valid;                      // Is the date valid?

// -------------------- Executable code section ----------------------

   char Ch = Read_token(In_stream, " ", " /-", Day_string);
   if (Ch == '/' || Ch == '-')
      Ch = Read_token(In_stream, "", " /-", Month_string);
   if (Ch == '/' || Ch == '-')
      Read_token(In_stream, "", " ", Year_string);

   Valid = (Day_string.length() > 0 && Month_string.length() > 0);
   if (Must_have_year)
      Valid = (Valid && Year_string.length() > 0);

   else if (Year_string.length() == 0)
      Year_string = NO_YEAR_STRING;

   if (Valid)
      {
      Day = atoi(Day_string.c_str());
      if (Is_numerical(Month_string.c_str()))
         Month = atoi(Month_string.c_str());
      else
         Month = Month_string_2_integer (Month_string);

      Year = atoi(Year_string.c_str());
      Set(Day, Month, Year);
      }
   else
      Julian_day = 0;
   }

// Identify one of these delimiters in a string
char FindDelimiter(const char *str)
{
	const char sep_char[] = {',','-','.',' ','/'};
	const char* sep = NULL;
	int i = 0;
	while ((sep == NULL) && (i < 5))
	{
		sep = strchr(str, sep_char[i]);
		if (sep)
			return *sep;
		i++;
	}
	return 0;
}

void GDate::Read(const string& Str, const string& Format)
   {
	string Fmt = Format;

	// if the delimiters in the date and format don't match
	// then do a substution
	char formatSep = FindDelimiter(Format.c_str());
	char inputSep =  FindDelimiter(Str.c_str());
	if (formatSep && inputSep)
	{
		if (inputSep != formatSep)
		{
			std::string f(&formatSep, 1);
			std::string s(&inputSep, 1);
			replaceAll(Fmt, f, s);
		}
	}
   
	// Need to create a sanitised format string i.e. from dd/mm/yyyy to %d/%m/%Y
   replaceAll(Fmt, "dd", "d");
   replaceAll(Fmt, "d", "%d");
   replaceAll(Fmt, "mm", "m");
   replaceAll(Fmt, "m", "%m");
   replaceAll(Fmt, "yyyy", "%Y");

   tm TM;
   if (strptime((char*) Str.c_str(), Fmt.c_str(), &TM) != NULL)
      Set(TM.tm_mday, TM.tm_mon+1, TM.tm_year);  // not sure why strptime as dates from 0 to 11.
   }
// *******************************************************************
       void GDate::Write(string &Str)  {
// *******************************************************************

//  Short description:
//    Write the date to the string using the current write_format

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables
//    none

// -------------------- Executable code section ----------------------

   ostringstream Out_stream;
   Write(Out_stream);
   Str = Out_stream.str();
   }

std::string GDate::ToString(std::string format)
{
   std::string current = Format_string;
   ostringstream Out_stream;
   if (format.length() > 0)
   {
	   Format_string = format;
   }
   Write(Out_stream);
   Format_string = current;

   return Out_stream.str();
}

// *******************************************************************
       void GDate::Write(ostream& Out_stream)  {
// *******************************************************************

//  Short description:
//    Write the date to the output stream

//  Notes:

//  Changes:
//    DPH 13/1/95
//    dph 6/8/97  changed to new formatting system.
//    dph 27/3/98 changed string::npos to string::npos in line with standard.

//  Calls:

//  Internal variables
      unsigned int Day, Month, Year;

// -------------------- Executable code section ----------------------

   string Our_string = Format_string;
   char St[50];

   if (Is_valid())
      {
      Get_dmy(Day, Month, Year);

      // replace DD
      size_t Pos = Our_string.find("DD");
      if (Pos != string::npos)
         {
         sprintf (St, "%02i", Day);
         Our_string.replace (Pos, 2, St);
         }

      // replace D
      Pos = Our_string.find("D");
      if (Pos != string::npos)
         {
         sprintf (St, "%i", Day);
         Our_string.replace (Pos, 1, St);
         }

      // replace MMMMMM
      Pos = Our_string.find("MMMMMM");
      if (Pos != string::npos)
         Our_string.replace (Pos, 6, Month_str[Month - 1]);
      else
         {
         // replace MMM
         Pos = Our_string.find("MMM");
         if (Pos != string::npos)
            {
            strncpy(St, Month_str[Month - 1], 3);
            St[3] = 0;
            Our_string.replace (Pos, 3, St);
            }
         else
            {
            // replace MM
            Pos = Our_string.find("MM");
            if (Pos != string::npos)
               {
               sprintf (St, "%02i", Month);
               Our_string.replace (Pos, 2, St);
               }
            else
               {
               // replace M
               Pos = Our_string.find("M");
               if (Pos != string::npos)
                  {
                  sprintf (St, "%i", Month);
                  Our_string.replace (Pos, 1, St);
                  }
               }
            }
         }

      // replace YYYY
      Pos = Our_string.find("YYYY");
      if (Pos != string::npos)
         {
         sprintf (St, "%i", Year);
         Our_string.replace (Pos, 4, St);
         }

      // replace YY
      Pos = Our_string.find("YY");
      if (Pos != string::npos)
         {
         sprintf (St, "%i", Year - 1900);
         Our_string.replace (Pos, 2, St);
         }
      }
   else
      Our_string = "dd/mm/yyyy";

   Out_stream << Our_string;
   }

// *******************************************************************
       unsigned int GDate::Get_days_in_month(unsigned int Month, unsigned int Year)  {
// *******************************************************************

//  Short description:
//    Return number of days in month.

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables

// -------------------- Executable code section ----------------------

   unsigned int Days_in_month = Acc_days_in_month[Month] - Acc_days_in_month[Month-1];
   if (Is_leap_year(Year) && Month == 2)
      Days_in_month++;

   return Days_in_month;
   }

// *******************************************************************
       bool GDate::Dmy_is_valid(unsigned int Day,
                                unsigned int Month,
                                unsigned int Year)  {
// *******************************************************************

//  Short description:
//    Return TRUE if date is valid.

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables
      bool is_valid;                   // Is the date valid?

// -------------------- Executable code section ----------------------

   if (Year < 100)
      Year += 1900;

   // Check for valid year.

   is_valid = (Year >= 1583 && Year < 4200);

   // Check for valid month

   if (is_valid)
      is_valid = (Month >= 1 && Month <= 12);

   // Check for valid day

   if (is_valid)
      {
      unsigned int Days_in_month = Get_days_in_month(Month, Year);
      is_valid = (Day >= 1 && Day <= Days_in_month);
      }

   return is_valid;
   }

// *******************************************************************
       bool GDate::Is_leap_year(int year)  {
// *******************************************************************

//  Short description:
//    Return TRUE if date is in a leap year

//  Notes:
//    Algorithm from K & R, "The C Programming Language", 1st ed.

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables
   int Yr;                        // Current year

// -------------------- Executable code section ----------------------

   if (year > 0)
      Yr = year;
   else
      Yr = Get_year();

   return ((Yr & 3) == 0 && Yr % 100 != 0) || (Yr % 400 == 0);
   }


// *******************************************************************
       void GDate::Get_dmy(unsigned int& d, unsigned int& m, unsigned int& y)  {
// *******************************************************************

//      This implementation is only valid for dates in the Gregorian
//      calender (after 15 October 1582).
//      THIS IS BASED ON THE ALGORITHM BY FLIEGEL AND VAN FLANDERN IN
//      C.ACM VOL.11 (OCT,1968) P.657

   if (Is_valid())
      {
      // fliegel and van flanden algorithm:

      double work = Julian_day + 68569.0;
      double work0 = int(4.0 * work / 146097.0);
      work = work - int((146097.0 * work0 + 3.0) /4.0);
      double yy = int(4000.0 * (work + 1.0) / 1461001.0);

      work = work - int(1461.0 * yy / 4.0) + 31.0;
      double mm = int(80.0 * work / 2447.0);
      double day = work - int(2447.0 * mm / 80.0);

      work = int(mm / 11.0);
      double month = mm + 2.0 - 12.0 * work;
      double year = 100.0 * (work0 - 49.0) + yy + work;

      d = (unsigned)day;
      m = (unsigned)month;
      y = (unsigned)year;
      }
   else
      {
      d = 0;
      m = 0;
      y = 0;
      }
   }

// *******************************************************************
       void GDate::Set(unsigned int d, unsigned int m, unsigned int y )  {
// *******************************************************************

//  Short description:
//      This implementation is only valid for dates in the Gregorian
//      calender (after 15 October 1582).
//      THIS IS BASED ON THE ALGORITHM BY FLIEGEL AND VAN FLANDERN IN
//      C.ACM VOL.11 (OCT,1968) P.657

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

// -------------------- Executable code section ----------------------

   if(Dmy_is_valid(d, m, y))
      {
      double Quotnt = int((m - 14.0) / 12.0);

      Julian_day = (unsigned int)(d - 32075.0 + int(1461.0 * (y + 4800.0 + Quotnt) /4.0)
                               + int( 367.0 * (m - 2.0    - Quotnt * 12.0) / 12.0)
                               - int(   3.0 * int((y + 4900.0 + Quotnt) / 100.0) /4.0));
      }
   else
      Julian_day = 0;
   }


// *******************************************************************
       void GDate::Set(unsigned int Day_of_year, unsigned int Year)  {
// *******************************************************************

//  Short description:
//    Set the julian day of this object from day of year and year

//  Notes:

//  Changes:
//    DPH 16/1/95

//  Calls:

//  Internal variables
      int Month = 11;                  // Month number
      int Day;                         // Day number
      bool Leap;                       // Is year a leap year?
// -------------------- Executable code section ----------------------

   Leap = ((Year & 3) == 0 && Year % 100 != 0) || (Year % 400 == 0);

   if (Leap && Day_of_year == 60)
      Set(29, 2, Year);
   else
      {
      // Calculate the month first.

      if (Leap && Day_of_year > 59)
         Day_of_year--;

      while (Month > 0 && Acc_days_in_month[Month] >= Day_of_year)
         Month--;
      Month++;

      // Now calculate the day number

      Day = Day_of_year - Acc_days_in_month[Month - 1];

      // Call the other set routine

      Set(Day, Month, Year);
      }
   }

// *******************************************************************
       void GDate::Set_to_today(void)  {
// *******************************************************************

//  Short description:
//    Set object to current date.

//  Notes:

//  Changes:
//    DPH 18/10/95

//  Calls:

//  Internal variables

// -------------------- Executable code section ----------------------

   // get todays date.
   time_t t;
   t = time(NULL);

   // Cvt to struct
   struct tm *ts = localtime(&t);


   Set(ts->tm_mday, ts->tm_mon+1, ts->tm_year+1900);
   }

// *******************************************************************
       unsigned int GDate::Get_day_of_year(void)  {
// *******************************************************************

//  Short description:
//    Return the day of year to caller

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables
      unsigned int Day, Month, Year;   // Current day, month and year
      unsigned int Day_of_year;        // Day of year to return to caller

// -------------------- Executable code section ----------------------

   if (Is_valid())
      {
      Get_dmy(Day, Month, Year);
      Day_of_year = Acc_days_in_month[Month - 1] + Day;

      if (Is_leap_year(Year) && Month > 2)
         Day_of_year++;
      }
   else
      Day_of_year = 0;

   return Day_of_year;
   }

// *******************************************************************
       unsigned int GDate::Get_day(void)  {
// *******************************************************************

//  Short description:
//    Return day

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables
      unsigned int Day, Month, Year;   // Current day, month and year

// -------------------- Executable code section ----------------------

   Get_dmy(Day, Month, Year);
   return Day;
   }

// *******************************************************************
       unsigned int GDate::Get_month(void)  {
// *******************************************************************

//  Short description:
//    Return day

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables
      unsigned int Day, Month, Year;   // Current day, month and year

// -------------------- Executable code section ----------------------

   Get_dmy(Day, Month, Year);
   return Month;
   }

// *******************************************************************
       unsigned int GDate::Get_year(void)  {
// *******************************************************************

//  Short description:
//    Return day

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables
      unsigned int Day, Month, Year;   // Current day, month and year

// -------------------- Executable code section ----------------------

   Get_dmy(Day, Month, Year);
   return Year;
   }

// *******************************************************************
       void GDate::Add_months(int Num_months)  {
// *******************************************************************

//  Short description:
//    Advance date to first of next month.

//  Notes:

//  Changes:
//    DPH 13/1/95

//  Calls:

//  Internal variables
      unsigned int Day, Month, Year;   // Current day, month and year

// -------------------- Executable code section ----------------------

   Get_dmy(Day, Month, Year);
   Month += Num_months;

   while (Month > 12)
      {
      Month -= 12;
      Year++;
      }
   Set(1, Month, Year);

   }


