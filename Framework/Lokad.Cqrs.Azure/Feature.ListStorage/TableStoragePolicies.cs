using System.Data.Services.Client;
using System.Text.RegularExpressions;

namespace Lokad.Cqrs.Feature.ListStorage
{
    static class TableStoragePolicies
    {

        public static string GetErrorCode(DataServiceRequestException ex)
        {
            var r = new Regex(@"<code>(\w+)</code>", RegexOptions.IgnoreCase);
            var match = r.Match(ex.InnerException.Message);
            return match.Groups[1].Value;
        }

        // HACK: just dupplicating the other overload of 'GetErrorCode'
        public static string GetErrorCode(DataServiceQueryException ex)
        {
            var r = new Regex(@"<code>(\w+)</code>", RegexOptions.IgnoreCase);
            var match = r.Match(ex.InnerException.Message);
            return match.Groups[1].Value;
        }
    }
}