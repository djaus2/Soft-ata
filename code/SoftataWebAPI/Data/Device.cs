﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftataWebAPI.Data.Db
{
    public class Device
    {
        public Device()
        {

        }

        [Key]
        public int Id { get; set; }
        public int SoftataId { get; set; }
        
        public string Name { get; set; }

        [ForeignKey("DtypeId")]
        public int DtypeId { get; set; }
        
        private DType _DType;
        private ILazyLoader LazyLoader { get; set; }
        public DType DType
        {
            get => LazyLoader.Load(this, ref _DType);
            set => _DType = value;
        }
    }
}
