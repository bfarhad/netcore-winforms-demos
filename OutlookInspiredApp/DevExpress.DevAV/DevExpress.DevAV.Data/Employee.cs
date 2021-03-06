﻿using DevExpress.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Runtime.Serialization;

namespace DevExpress.DevAV {
    public enum EmployeeStatus {
        [Display(Name = "Salaried")]
        Salaried,
        [Display(Name = "Commission")]
        Commission,
        [Display(Name = "Contract")]
        Contract,
        [Display(Name = "Terminated")]
        Terminated,
        [Display(Name = "On Leave")]
        OnLeave
    }
    public enum EmployeeDepartment {
        [Display(Name = "Sales")]
        Sales = 1,
        [Display(Name = "Support")]
        Support,
        [Display(Name = "Shipping")]
        Shipping,
        [Display(Name = "Engineering")]
        Engineering,
        [Display(Name = "Human Resources")]
        HumanResources,
        [Display(Name = "Management")]
        Management,
        [Display(Name = "IT")]
        IT
    }
    public enum PersonPrefix {
        Dr,
        Mr,
        Ms,
        Miss,
        Mrs
    }
    public partial class Employee : DatabaseObject {
        public Employee() {
            AssignedTasks = new List<EmployeeTask>();
            OwnedTasks = new List<EmployeeTask>();
#if DXCORE3
            _address = new Address();
            _address.PropertyChanged += (s, e) => SetPropertyValue(e.PropertyName, "Address", (Address)s);
#else
            Address = new Address();
#endif
            AssignedEmployeeTasks = new List<EmployeeTask>();
        }
        [InverseProperty("AssignedEmployees")]
        public virtual List<EmployeeTask> AssignedEmployeeTasks { get; set; }
        public EmployeeDepartment Department { get; set; }
        [Required]
        public string Title { get; set; }
        public EmployeeStatus Status { get; set; }
        [Display(Name = "Hire Date")]
        public DateTime? HireDate { get; set; }
        [InverseProperty("AssignedEmployee")]
        public virtual List<EmployeeTask> AssignedTasks { get; set; }
        [InverseProperty("Owner")]
        public virtual List<EmployeeTask> OwnedTasks { get; set; }
        [InverseProperty("Employee")]
        public virtual List<Evaluation> Evaluations { get; set; }
        public string PersonalProfile { get; set; }
        public long? ProbationReason_Id { get; set; }
        public virtual Probation ProbationReason { get; set; }
        [Required, Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required, Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        public PersonPrefix Prefix { get; set; }
        [Phone, Display(Name = "Home Phone")]
        public string HomePhone { get; set; }
        [Required, Phone, Display(Name = "Mobile Phone")]
        public string MobilePhone { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        public string Skype { get; set; }
        [Display(Name = "Birth Date")]
        public DateTime? BirthDate { get; set; }
        public virtual Picture Picture { get; set; }
        public long? PictureId { get; set; }
#if DXCORE3
        Address _address;
        [NotMapped]
        public Address Address {
            get {
                AddressHelper.UpdateAddress(_address, AddressLine, AddressCity, AddressState, AddressZipCode, AddressLatitude, AddressLongitude);
                return _address;
            }
            set { AddressHelper.UpdateAddress(_address, value.Line, value.City, value.State, value.ZipCode, value.Latitude, value.Longitude); }
        }
#else
        public Address Address { get; set; }
#endif
#if ONGENERATEDATABASE || DXCORE3
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string AddressLine { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string AddressCity { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public StateEnum AddressState { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string AddressZipCode { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public double AddressLatitude { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public double AddressLongitude { get; set; }
#endif

        Image _photo = null;
        [NotMapped]
        public Image Photo {
            get {
                if(_photo == null)
                    _photo = Picture.CreateImage();
                return _photo;
            }
            set {
                if(_photo == value) return;
                if(_photo != null)
                    _photo.Dispose();
                _photo = value;
                Picture = PictureExtension.FromImage(value);
            }
        }
        bool unsetFullName = false;
        public virtual ICollection<Evaluation> EvaluationsCreatedBy { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Product> SupportedProducts { get; set; }
        public virtual ICollection<Quote> Quotes { get; set; }
        public virtual ICollection<CustomerCommunication> Employees { get; set; }
        [NotMapped, Display(Name = "Full Name")]
        public string FullNameBindable {
            get {
                return string.IsNullOrEmpty(FullName) || unsetFullName ? GetFullName() : FullName;
            }
            set {
                unsetFullName = string.IsNullOrEmpty(value);
                if(unsetFullName)
                    FullName = GetFullName();
                else
                    FullName = value;
            }
        }
        public void ResetBindable() {
            if(_photo != null)
                _photo.Dispose();
            _photo = null;
            unsetFullName = false;
        }
        string GetFullName() {
            return string.Format("{0} {1}", FirstName, LastName);
        }
        public override string ToString() {
            return FullName;
        }
    }
}