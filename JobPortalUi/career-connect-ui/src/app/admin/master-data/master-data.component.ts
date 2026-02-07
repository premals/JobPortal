import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { adminMasterData, addAuditLog } from '../admin-data';

type MasterKey = 'skills' | 'industries' | 'locations' | 'employmentTypes' | 'workModes';

@Component({
  standalone: true,
  selector: 'app-master-data',
  imports: [CommonModule, FormsModule],
  templateUrl: './master-data.component.html',
  styleUrls: ['./master-data.component.scss']
})
export class MasterDataComponent {
  masterData = adminMasterData;

  sections: Array<{ key: MasterKey; label: string; placeholder: string }> = [
    { key: 'skills', label: 'Skills', placeholder: 'Add a skill' },
    { key: 'industries', label: 'Industries', placeholder: 'Add an industry' },
    { key: 'locations', label: 'Locations', placeholder: 'Add a location' },
    { key: 'employmentTypes', label: 'Employment Types', placeholder: 'Add employment type' },
    { key: 'workModes', label: 'Work Modes', placeholder: 'Add work mode' }
  ];

  newItem: Record<MasterKey, string> = {
    skills: '',
    industries: '',
    locations: '',
    employmentTypes: '',
    workModes: ''
  };

  addItem(sectionKey: MasterKey): void {
    const value = this.newItem[sectionKey]?.trim();
    if (!value) return;
    const list = this.masterData[sectionKey];
    if (list.includes(value)) return;
    list.push(value);
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Add Master Data',
      entityType: sectionKey,
      entityId: value,
      newValue: value,
      reason: 'Admin update'
    });
    this.newItem[sectionKey] = '';
  }

  removeItem(sectionKey: MasterKey, value: string): void {
    const list = this.masterData[sectionKey];
    const index = list.indexOf(value);
    if (index === -1) return;
    list.splice(index, 1);
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Remove Master Data',
      entityType: sectionKey,
      entityId: value,
      oldValue: value,
      reason: 'Admin update'
    });
  }
}

