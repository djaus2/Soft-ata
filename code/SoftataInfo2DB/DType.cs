using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace SoftataInfo2DB
{
    public class DType
    {
        public DType()
        {

        }

        ILazyLoader _lazyLoader;

        private DType(ILazyLoader lazyLoader)
        {
            _lazyLoader = lazyLoader;
        }

        [Key]
        public int Id { get; set; }
        public int SoftataId { get; set; }
        public string Name { get; set; }

        private ICollection<Device> _Devices;
        public ICollection<Device> Devices
        {
            get => _lazyLoader.Load(this, ref _Devices) ?? _Devices;
            set => _Devices = value;
        }

        public ICollection<GenericCommand> _Commands;
        public ICollection<GenericCommand> Commands
        {
            get => _lazyLoader.Load(this, ref _Commands) ?? _Commands;
            set => _Commands = value;
        }
    }
}

