import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { Company } from '../../interfaces/company';
import { NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterOutlet } from '@angular/router';
import { StateService } from '../../services/state.service';
import { UsersService } from '../../apiservices/users.service';
import { CompaniesService } from '../../apiservices/companies.service';

@Component({
  selector: 'app-companies',
  standalone: true,
  imports: [FormsModule, NgIf, NgFor, RouterLink, RouterOutlet],
  templateUrl: './companies.component.html',
  styleUrl: './companies.component.css'
})
export class CompaniesComponent implements OnInit {

  public companies: Company[] | null = null;

  @Output() updateEvent = new EventEmitter();
  
  constructor(
    private companiesService: CompaniesService,
    public stateService: StateService,
    private userService: UsersService) { }

  getCurrentCompanyId() {
    const state = this.stateService.getState();
    return state?.currentCompany?.id;
  }

  ngOnInit(): void {
    const state = this.stateService.getState();
    this.companiesService
      .list({
        bearerId: state?.bearerId ?? null,
        currentCompanyId: state?.currentCompany?.id ?? null,
      })
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.companies = response.companies;
          }
        },
        error: (error) => {
          console.error('List companies failed:', error);
        }
      });

  }
  setCurrentCompany(companyid: number) {
    const state = this.stateService.getState();
    this.userService
      .setcurrentcompany({
        bearerId: state?.bearerId ?? null,
        currentCompanyId: companyid
      })
      .subscribe({
        next: (response) => {
          this.stateService.setState(response.state);
          this.updateEvent.emit();
        },
        error: (error) => {
          console.error('List companies failed:', error);
        }
      });
  }
}