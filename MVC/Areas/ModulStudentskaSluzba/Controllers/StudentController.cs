using MVC.DAL;
using MVC.Models;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC.Areas.ModulStudentskaSluzba.Data;

namespace MVC.Areas.ModulStudentskaSluzba.Controllers
{
    public class StudentController : Controller
    {
        MyContext mc = new MyContext();

        // defaultna akcija (kada unesemo samo http://localhost:50913/Student/)
        public ActionResult Index()
        {
            return View("Index");
        }

        // pristupamo sa: http://localhost:50913/Student/Prikazi)
        public ActionResult Prikazi(int? smjerId)   // mozemo prikazati sve studente bez zadavanja smjera (null)
        {
            StudentPrikaziVM Model = new StudentPrikaziVM();

            Model.studenti = mc.Studenti
              .Where(x => !smjerId.HasValue || x.SmjerId == smjerId)     // ako smjerId nema vrijednost (ako je null), drugi uslov se nece ni gledati, pa ce vratiti sve studente
              .Select(x => new StudentPrikaziVM.StudenInfo     // object initializer
              {
                  // Student = x,     // kako u ViewModelu vise ne koristimo domain model tipa Student nego njegove atribute, iste je potrebno inicijalizovat
                  BrojIndexa = x.BrojIndexa,
                  Fakultet_Naziv = x.Smjer.Fakultet.Naziv,
                  Id = x.Id,
                  Ime = x.Korisnik.Ime,
                  Prezime = x.Korisnik.Prezime,
                  Smjer_Naziv = x.Smjer.Naziv,

                  ECTSukupno = mc.SlusaPredmete.Where(y => y.UpisGodine.StudentId == x.Id && y.FinalnaOcjena > 5).Sum(z => (float?) z.Predaje.Predmet.Ects) ?? 0, // float? jer moze da ne vrati nista (npr. ECTS nije unesen)
                  BrojPolozenihPredmeta = mc.SlusaPredmete.Where(y => y.UpisGodine.StudentId == x.Id && y.FinalnaOcjena > 5).Count()
              })
                .ToList();

            ////////////////////////////////

            Model.smjeroviStavke = UcitajSmjerove();

            return View("Prikazi", Model);
        }

        public ActionResult Obrisi(int studentId)
        {
            Student s = mc.Studenti.Where(x => x.Id == studentId).Include(x => x.Korisnik).FirstOrDefault();
            mc.Studenti.Remove(s);
            mc.SaveChanges();

            return RedirectToAction("Prikazi");
        }

        //  http://localhost:50913/Student/Dodaj
        public ActionResult Dodaj()
        {
            StudentUrediVM Model = new StudentUrediVM();

            Model.smjeroviStavke = UcitajSmjerove();

            return View("Uredi", Model);
        }

        public ActionResult Uredi(int studentId)
        {
            Student student = mc.Studenti.Where(x => x.Id == studentId).Include(x => x.Korisnik).FirstOrDefault();      // Include() ne radi sa Find() pa moramo koristiti Where()

            // 1. inicijalizacija objekta Model koristenjem inicijalizatora
            //StudentUrediViewModel Model = new StudentUrediViewModel
            //{
            //    Id = student.Id,
            //    Ime = student.Korisnik.Ime,
            //    Prezime = student.Korisnik.Prezime,
            //    Username = student.Korisnik.Username,
            //    Password = student.Korisnik.Password,
            //    SmjerId = student.SmjerId,
            //    BrojIndexa = student.BrojIndexa,
            //    DatumRodjenja = student.Korisnik.DatumRodjenja
            //};

            // 2.inicijalizacija objekta model na klasicni nacin
            StudentUrediVM Model = new StudentUrediVM();
            Model.Id = student.Id;
            Model.Ime = student.Korisnik.Ime;
            Model.Prezime = student.Korisnik.Prezime;
            Model.Username = student.Korisnik.Username;
            Model.Password = student.Korisnik.Password;
            Model.SmjerId = student.SmjerId;
            Model.BrojIndexa = student.BrojIndexa;
            Model.DatumRodjenja = student.Korisnik.DatumRodjenja;
            Model.smjeroviStavke = UcitajSmjerove();

            return View("Uredi", Model);
        }


        public ActionResult Snimi(StudentUrediVM student)
        {
            // ako validacija nije ispravna
            if (!ModelState.IsValid)
            {
                // Prije nego posaljemo objekat "student" nazad na View, moramo napuniti "smjeroviStavke" atribut jer objekat "student" ima samo vlasititi SmjerId.
                student.smjeroviStavke = UcitajSmjerove();

                return View("Uredi", student);
            }

            Student studentDB;

            if (student.Id == 0)
            {
                studentDB = new Student();
                studentDB.Korisnik = new Korisnik();
                mc.Studenti.Add(studentDB);     // Mozemo prvo dodati objekat pa ga onda setovat i obrnuto - redoslijed nije bitan (objekat se nalazi u memoriji)
            }
            else
            {
                studentDB = mc.Studenti.Where(x => x.Id == student.Id).Include(x => x.Korisnik).FirstOrDefault();
            }

            // setovanje objekta
            studentDB.Korisnik.Ime = student.Ime;
            studentDB.Korisnik.Prezime = student.Prezime;
            studentDB.Korisnik.Username = student.Username;
            studentDB.Korisnik.Password = student.Password;
            studentDB.Korisnik.DatumRodjenja = student.DatumRodjenja;
            studentDB.BrojIndexa = student.BrojIndexa;
            studentDB.SmjerId = student.SmjerId;

            mc.SaveChanges();   // snima objekat u bazu

            return RedirectToAction("Prikazi");
        }

        // Ucitaje sve smjerove plus jedno defaultno polje "Svi smjerovi"
        private List<SelectListItem> UcitajSmjerove()
        {
            var smjeroviStavke = new List<SelectListItem>();
            smjeroviStavke.Add(new SelectListItem { Value = null, Text = "Svi smjerovi" });
            smjeroviStavke.AddRange(mc.Smjerovi.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Fakultet.Naziv + " " + x.Naziv }).ToList());
            return smjeroviStavke;
        }
    }
}