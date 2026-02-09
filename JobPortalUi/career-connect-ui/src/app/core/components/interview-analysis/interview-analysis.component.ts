import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InterviewAnalysis, InterviewIntegrityEvent } from '../../models/job-seeker/interview.model';

@Component({
  selector: 'app-interview-analysis',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="analysis-container">
      <div class="analysis-header">
        <h2>Interview Analysis & Report</h2>
        <p class="generated-at">Generated: {{ formatDate(analysis.generatedAt) }}</p>
      </div>

      <div class="score-section">
        <div class="main-score">
          <div class="score-circle">
            <svg viewBox="0 0 100 100" class="progress-ring">
              <circle
                cx="50" cy="50" r="45"
                [style.stroke-dasharray]="circumference"
                [style.stroke-dashoffset]="circumference - (analysis.overallScore / 100) * circumference"
                class="progress-ring-circle"
              />
            </svg>
            <div class="score-text">
              <span class="score-value">{{ analysis.overallScore }}</span>
              <span class="score-label">out of 100</span>
            </div>
          </div>

          <div class="score-breakdown">
            <div class="score-item">
              <div class="score-label-small">Communication</div>
              <div class="score-bar">
                <div class="score-fill" [style.width.%]="analysis.communicationScore"></div>
              </div>
              <div class="score-number">{{ analysis.communicationScore }}/100</div>
            </div>

            <div class="score-item">
              <div class="score-label-small">Technical</div>
              <div class="score-bar">
                <div class="score-fill" [style.width.%]="analysis.technicalScore"></div>
              </div>
              <div class="score-number">{{ analysis.technicalScore }}/100</div>
            </div>

            <div class="score-item">
              <div class="score-label-small">Confidence</div>
              <div class="score-bar">
                <div class="score-fill" [style.width.%]="analysis.confidenceScore"></div>
              </div>
              <div class="score-number">{{ analysis.confidenceScore }}/100</div>
            </div>
          </div>
        </div>
      </div>

      <section class="analysis-section">
        <h3>Summary</h3>
        <div class="summary-box">
          <p>{{ analysis.summary }}</p>
        </div>
      </section>

      <section class="analysis-section" *ngIf="integrityEvents && integrityEvents.length > 0">
        <h3>Integrity Alerts</h3>
        <div class="integrity-list">
          <div *ngFor="let event of integrityEvents" class="integrity-item">
            <span class="integrity-time">{{ formatDate(event.timestamp) }}</span>
            <span class="integrity-message">{{ event.message }}</span>
          </div>
        </div>
      </section>

      <section class="analysis-section">
        <h3>Key Strengths</h3>
        <ul class="strengths-list">
          <li *ngFor="let strength of analysis.keyStrengths" class="strength-item">
            <span class="icon">✓</span>
            <span>{{ strength }}</span>
          </li>
        </ul>
      </section>

      <section class="analysis-section">
        <h3>Areas for Improvement</h3>
        <ul class="improvements-list">
          <li *ngFor="let improvement of analysis.areasForImprovement" class="improvement-item">
            <span class="icon">→</span>
            <span>{{ improvement }}</span>
          </li>
        </ul>
      </section>

      <section class="analysis-section" *ngIf="analysis.recommendedQuestions && analysis.recommendedQuestions.length > 0">
        <h3>Recommended Practice Questions</h3>
        <div class="questions-list">
          <div *ngFor="let question of analysis.recommendedQuestions" class="question-item">
            <p>{{ question }}</p>
          </div>
        </div>
      </section>

      <section class="analysis-section" *ngIf="analysis.nextSteps">
        <h3>Next Steps</h3>
        <div class="next-steps-box">
          <p>{{ analysis.nextSteps }}</p>
        </div>
      </section>

      <div class="action-buttons">
        <button class="btn btn-primary" (click)="downloadReport()">Download Report</button>
        <button class="btn btn-secondary" (click)="shareReport()">Share Report</button>
      </div>
    </div>
  `,
  styles: [`
    .analysis-container {
      background: white;
      border-radius: 18px;
      box-shadow: 0 18px 36px rgba(15, 23, 42, 0.12);
      padding: 32px;
      max-width: 900px;
      margin: 0 auto;
      font-family: "Sora", "Segoe UI", sans-serif;
    }

    .analysis-header {
      text-align: center;
      margin-bottom: 32px;
    }

    .analysis-header h2 {
      margin: 0 0 8px 0;
      font-size: 26px;
      font-weight: 700;
      color: #0f172a;
    }

    .generated-at {
      margin: 0;
      color: #94a3b8;
      font-size: 13px;
    }

    .score-section {
      background: linear-gradient(135deg, #0f766e 0%, #0ea5e9 55%, #22c55e 100%);
      padding: 32px;
      border-radius: 16px;
      margin-bottom: 32px;
      color: white;
    }

    .main-score {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 32px;
      align-items: center;
    }

    .score-circle {
      position: relative;
      width: 200px;
      height: 200px;
      margin: 0 auto;
    }

    .progress-ring {
      transform: rotate(-90deg);
    }

    .progress-ring-circle {
      fill: none;
      stroke: rgba(255, 255, 255, 0.3);
      stroke-width: 8;
      transition: stroke-dashoffset 0.35s;
      stroke-linecap: round;
    }

    .score-text {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      text-align: center;
    }

    .score-value {
      display: block;
      font-size: 46px;
      font-weight: 700;
    }

    .score-label {
      display: block;
      font-size: 12px;
      opacity: 0.9;
    }

    .score-breakdown {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .score-item {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    .score-label-small {
      font-size: 13px;
      font-weight: 600;
      opacity: 0.9;
    }

    .score-bar {
      background: rgba(255, 255, 255, 0.3);
      height: 8px;
      border-radius: 4px;
      overflow: hidden;
    }

    .score-fill {
      background: #facc15;
      height: 100%;
      border-radius: 4px;
      transition: width 0.5s ease;
    }

    .score-number {
      font-size: 12px;
      font-weight: 600;
    }

    .analysis-section {
      margin-bottom: 28px;
    }

    .analysis-section h3 {
      margin: 0 0 12px 0;
      font-size: 17px;
      font-weight: 600;
      color: #0f172a;
      border-bottom: 2px solid #0ea5e9;
      padding-bottom: 8px;
    }

    .summary-box {
      background: #f8fafc;
      padding: 16px;
      border-left: 4px solid #0ea5e9;
      border-radius: 6px;
      line-height: 1.7;
      color: #475569;
    }

    .integrity-list {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .integrity-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
      background: #fff7ed;
      padding: 12px;
      border-radius: 10px;
      border-left: 4px solid #f97316;
      color: #7c2d12;
    }

    .integrity-time {
      font-size: 12px;
      font-weight: 600;
      color: #c2410c;
    }

    .integrity-message {
      font-size: 14px;
      color: #7c2d12;
    }

    .strengths-list,
    .improvements-list {
      list-style: none;
      padding: 0;
      margin: 0;
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .strength-item,
    .improvement-item {
      display: flex;
      gap: 12px;
      padding: 12px;
      background: #f8fafc;
      border-radius: 10px;
      align-items: flex-start;
    }

    .strength-item .icon {
      color: #16a34a;
      font-weight: 700;
      flex-shrink: 0;
      margin-top: 2px;
    }

    .improvement-item .icon {
      color: #f59e0b;
      font-weight: 700;
      flex-shrink: 0;
      margin-top: 2px;
    }

    .questions-list {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
      gap: 14px;
    }

    .question-item {
      background: #ecfeff;
      padding: 14px;
      border-radius: 10px;
      border-left: 3px solid #0ea5e9;
    }

    .question-item p {
      margin: 0;
      color: #0f172a;
      line-height: 1.6;
    }

    .next-steps-box {
      background: #ecfdf5;
      padding: 16px;
      border-left: 4px solid #22c55e;
      border-radius: 6px;
      line-height: 1.7;
      color: #166534;
    }

    .action-buttons {
      display: flex;
      gap: 12px;
      justify-content: center;
      margin-top: 32px;
      padding-top: 20px;
      border-top: 1px solid #e2e8f0;
    }

    .btn {
      padding: 12px 24px;
      border: none;
      border-radius: 999px;
      cursor: pointer;
      font-weight: 600;
    }

    .btn-primary {
      background: linear-gradient(120deg, #14b8a6, #0ea5e9);
      color: white;
    }

    .btn-secondary {
      background: #e2e8f0;
      color: #1f2937;
    }

    @media (max-width: 768px) {
      .analysis-container {
        padding: 20px;
      }

      .main-score {
        grid-template-columns: 1fr;
        gap: 20px;
      }

      .score-circle {
        width: 150px;
        height: 150px;
      }

      .score-value {
        font-size: 36px;
      }

      .action-buttons {
        flex-direction: column;
      }

      .btn {
        width: 100%;
      }
    }
  `]
})
export class InterviewAnalysisComponent implements OnInit {
  @Input() analysis!: InterviewAnalysis;
  @Input() integrityEvents: InterviewIntegrityEvent[] = [];

  circumference = 2 * Math.PI * 45;

  ngOnInit(): void {
    if (!this.analysis) {
      console.error('InterviewAnalysis component requires analysis input');
    }
  }

  downloadReport(): void {
    const reportContent = this.generateReportText();
    const element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(reportContent));
    element.setAttribute('download', `interview_analysis_${Date.now()}.txt`);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
  }

  shareReport(): void {
    const reportContent = this.generateReportText();
    if (navigator.share) {
      navigator.share({
        title: 'Interview Analysis Report',
        text: reportContent
      }).catch(err => console.log('Error sharing:', err));
    } else {
      navigator.clipboard.writeText(reportContent);
      alert('Report copied to clipboard');
    }
  }

  private generateReportText(): string {
    return `
INTERVIEW ANALYSIS REPORT
Generated: ${new Date(this.analysis.generatedAt).toLocaleString()}

OVERALL SCORE: ${this.analysis.overallScore}/100

BREAKDOWN:
- Communication: ${this.analysis.communicationScore}/100
- Technical: ${this.analysis.technicalScore}/100
- Confidence: ${this.analysis.confidenceScore}/100

SUMMARY:
${this.analysis.summary}

${this.integrityEvents?.length ? `INTEGRITY ALERTS:\n${this.integrityEvents.map(event => `- ${this.formatDate(event.timestamp)}: ${event.message}`).join('\n')}\n\n` : ''}

KEY STRENGTHS:
${this.analysis.keyStrengths.map(s => `- ${s}`).join('\n')}

AREAS FOR IMPROVEMENT:
${this.analysis.areasForImprovement.map(a => `- ${a}`).join('\n')}

${this.analysis.recommendedQuestions?.length ? `RECOMMENDED PRACTICE QUESTIONS:\n${this.analysis.recommendedQuestions.map(q => `- ${q}`).join('\n')}` : ''}

${this.analysis.nextSteps ? `NEXT STEPS:\n${this.analysis.nextSteps}` : ''}
    `;
  }

  formatDate(date: Date | string): string {
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
