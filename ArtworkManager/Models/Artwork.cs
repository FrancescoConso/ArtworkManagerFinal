using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkManager.Models
{
    public class Artwork
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titolo { get; set; }

        //rappresenta la stringa del file immagine memorizzato nel server
        [Required]
        public string Immagine { get; set; }

        [Required]
        public string Artista { get; set; }

        [Required]
        public string Stile { get; set; }

        [Required]
        public string Periodo { get; set; }

        public bool DaRimuovere { get; set; }

    }
}
