﻿namespace Employee_Lookup.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }
    }
}