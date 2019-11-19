using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UxoLoggingv2.Utils
{
    public class DatabaseConstants
    {
        public const string Uxo_Connection = "https://cortanadiagnosticslogs.kusto.windows.net/";
        public const string Uxo_Database = "CompliantCortana";

        public const string Cortex_Connection = "https://dialog.kusto.windows.net/";
        public const string Cortex_PPE_Database = "063b547f5df54cc384402b4caf7e69e9";
        public const string Cortex_Prod_Database = "3ae4902fc21442db90f082e42fed45e4";

        public const string Speech_Connection = "https://speech.kusto.windows.net/";
        public const string Speech_Database = "36f58762c2064953ae97d1c12318cde3";

    }
}
