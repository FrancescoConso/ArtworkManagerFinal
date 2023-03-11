using ArtworkManager.Data;
using ArtworkManager.Models;
using ArtworkManager.ViewModels;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkManager.Controllers
{
    public class ArtworksController : Controller
    {

        private readonly ArtworkContext context;
        private readonly IWebHostEnvironment hostEnvironment;

        public ArtworksController(ArtworkContext context, IWebHostEnvironment hostEnvironment)
        {
            this.context = context;
            this.hostEnvironment = hostEnvironment;
        }

        //GET di tutti gli artwork
        public async Task<IActionResult> Index()
        {
            List<Artwork> allArtworks = await context.Artworks.ToListAsync();
            List<Artwork> sortedList = allArtworks.OrderBy(o => o.Titolo).ToList();
            return View(sortedList);
        }

        //Apro la vista di creazione nuovo artwork
        public IActionResult NewArtwork()
        {
            return View();
        }

        //POST di un nuovo artwork
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArtworkViewModel artworkView)
        {
            if (ModelState.IsValid)
            {
                string serverFileName = null;   //nome del file sul server
                string percorsoFileImmagine = null; //percorso del file sul server
                try
                {  
                    //processiamo la stringa dell upload immagine
                    if (artworkView.Immagine != null)
                    {
                        //Come buona norma si prepende una stringa casuale con la classe guid
                        //e si prepende al filename
                        string directoryImmagini = Path.Combine(hostEnvironment.WebRootPath, "artworks");
                        serverFileName = Guid.NewGuid().ToString() + artworkView.Immagine.FileName;
                        percorsoFileImmagine = Path.Combine(directoryImmagini, serverFileName);

                        //copio l'immagine sul server
                        using (FileStream fileStream = new FileStream(percorsoFileImmagine, FileMode.Create))
                        {
                            artworkView.Immagine.CopyTo(fileStream);
                        }
  
                    }

                    //con l'immagine processata creo un nuovo artwork e lo aggiungo
                    Artwork artworkToAdd = new Artwork
                    {
                        Titolo = artworkView.Titolo,
                        Immagine = serverFileName,
                        Artista = artworkView.Artista,
                        Stile = artworkView.Stile,
                        Periodo = artworkView.Periodo,
                    };

                    context.Add(artworkToAdd);
                    await context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception e)
                {
                    //qualora fallisse un'operazione cancello il file dal server
                    //prima di sollevare l'eccezione
                    if (System.IO.File.Exists(percorsoFileImmagine))
                    {
                        System.IO.File.Delete(percorsoFileImmagine);
                    }

                    throw new Exception(e.Message);
                }

            }
            return RedirectToAction(nameof(NewArtwork));
        }

        //Apro la vista di dettaglio Artwork
        public async Task<IActionResult> DetailsArtwork(int? id)
        {
            if (id != null)
            {
                Artwork foundArtwork = await context.Artworks.FirstOrDefaultAsync(m => m.Id == id);
                if (foundArtwork != null)
                {
                    return View(foundArtwork);
                }
            }
            return NotFound();
        }

        //Apro la vista di modifica artwork
        public async Task<IActionResult> EditArtwork(int? id)
        {

            if (id != null)
            {
                Artwork foundArtwork = await context.Artworks.FirstOrDefaultAsync(m => m.Id == id);
                if (foundArtwork != null)
                {
                    return View(foundArtwork);
                }
            }
            return NotFound();
        }

        //POST di un artwork modificato
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int Id, ArtworkViewModel editedArtwork, IFormFile img)
        {

            if (Id != editedArtwork.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //recupero le informazioni dell artwork
                    Artwork dettagliVecchioArtwork = await context.Artworks.FirstOrDefaultAsync(m => m.Id == Id);
                    string immagineVecchioArtwork = dettagliVecchioArtwork.Immagine;

                    string serverFileName = null;
                    //processiamo la stringa della nuova immagine per salvarla
                    string directoryImmagini = Path.Combine(hostEnvironment.WebRootPath, "artworks");
                    serverFileName = Guid.NewGuid().ToString() + "_" + editedArtwork.Immagine.FileName;
                    string percorsoFileImmagine = Path.Combine(directoryImmagini, serverFileName);

                    using (FileStream fileStream = new FileStream(percorsoFileImmagine, FileMode.Create))
                    {
                        editedArtwork.Immagine.CopyTo(fileStream);
                    }

                    //salviamo il contesto del database
                    //aggiorniamo i campi
                    dettagliVecchioArtwork.Titolo = editedArtwork.Titolo;
                    dettagliVecchioArtwork.Immagine = serverFileName;
                    dettagliVecchioArtwork.Artista = editedArtwork.Artista;
                    dettagliVecchioArtwork.Stile = editedArtwork.Stile;
                    dettagliVecchioArtwork.Periodo = editedArtwork.Periodo;

                    context.Update(dettagliVecchioArtwork);
                    await context.SaveChangesAsync();

                    //e cancelliamo il vecchio file

                    string percorsoVecchiaImmagine = Path.Combine(directoryImmagini, immagineVecchioArtwork);
                    if (System.IO.File.Exists(percorsoVecchiaImmagine))
                    {
                        System.IO.File.Delete(percorsoVecchiaImmagine);
                    }


                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(EditArtwork), new { Id = Id });
        }

        //Apro la vista di conferma rimozione Artwork
        public async Task<IActionResult> DeleteArtwork(int? id)
        {
            if (id != null)
            {
                Artwork foundArtwork = await context.Artworks.FirstOrDefaultAsync(m => m.Id == id);
                if (foundArtwork != null)
                {
                    return View(foundArtwork);
                }
            }
            return NotFound();        
        }


        //POST di rimozione di un artwork
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? Id)
        {
            //non lo cancelliamo dal database ma impostiamo
            //il campo da rimuovere a true
            //hangfire si occupa di rimuoverlo
            if (Id != null)
            {
                Artwork foundArtwork = await context.Artworks.FirstOrDefaultAsync(m => m.Id == Id);
                if (foundArtwork != null)
                {
                    try
                    {
                        //aggiorniamo la riga del database
                        foundArtwork.DaRimuovere = true;

                        context.Update(foundArtwork);
                        await context.SaveChangesAsync();

                        //lanciamo l'operazione fire and forget di hangfire
                        BackgroundJob.Enqueue(() => DeleteFromDatabase(Id));

                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            return NotFound();
        }

        //Operazione Hangfire di rimozione di un database
        public async Task DeleteFromDatabase(int? Id)
        {
            //qui eseguo la cancellazione della riga del database con Id noto
            if (Id != null)
            {
                try
                {
                    Artwork foundArtwork = await context.Artworks.FirstOrDefaultAsync(m => m.Id == Id);
                    if (foundArtwork != null)
                    {
                        string fileNameDaRimuovere = foundArtwork.Immagine;

                        if (foundArtwork.DaRimuovere == true)
                            context.Remove(foundArtwork);
                        await context.SaveChangesAsync();

                        //cancelliamo l'immagine dal database
                        string directoryImmagini = Path.Combine(hostEnvironment.WebRootPath, "artworks");
                        string percorsoFileImmagine = Path.Combine(directoryImmagini, fileNameDaRimuovere);

                        if (System.IO.File.Exists(percorsoFileImmagine))
                        {
                            System.IO.File.Delete(percorsoFileImmagine);
                        }

                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

    }
}
