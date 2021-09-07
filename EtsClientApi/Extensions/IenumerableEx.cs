using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.Extensions
{
    public static class IenumerableEx
    {

       

            public static IEnumerable<DateTime> GetDatesOfWeek(this DateTime date, CultureInfo ci)
            {
                Int32 firstDayOfWeek = (Int32)ci.DateTimeFormat.FirstDayOfWeek;
                Int32 dayOfWeek = (Int32)date.DayOfWeek;
                DateTime startOfWeek = date.AddDays(firstDayOfWeek - dayOfWeek);
                var valuesDaysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<Int32>();
                return valuesDaysOfWeek.Select(v => startOfWeek.AddDays(v));
            }
        
    }
}
