using System;
using System.Collections.Generic;
using System.Text;

namespace StudentInfo.DTO
{
    public  class MessageResponse
    {
        public MessageResponse(string message="") { Message = message; }
        public string? Message { get; set; }
    }
}
