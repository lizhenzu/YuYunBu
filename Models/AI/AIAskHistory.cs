using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class AIAskHistory
    {
        public int Id { get; set; }
        public string WhatAsk { get; set; }
        public string UserNameKor { get; set; }
        public string UserName { get; set; }
        public string responseData { get; set; }
        public double durationTime { get; set; }
        public DateTime InsertDT { get; set; }
     
    }
}
