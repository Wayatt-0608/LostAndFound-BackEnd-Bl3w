using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostAndFound.Application.DTOs.MasterData;

public class CreateCampusRequest
{
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
}


