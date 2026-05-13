using System;
using System.Collections.Generic;
using System.Text;

namespace Message_Parser.Data
{
    public class SessionWithHitPage
    {
        public long Id { get; set; }
        public string Session_Id { get; set; }
        public string Site { get; set; }
        public string User_Id { get; set; }
        public string User_Ip { get; set; }
        public string Language_Browser { get; set; }
        public string User_Agent { get; set; }
        public DateTime Session_Start { get; set; }
        public DateTime Session_End { get; set; }
        public long HitPageId { get; set; }
    }
}
