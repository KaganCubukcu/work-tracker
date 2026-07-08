import { Component, inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-lang-switcher',
  standalone: true,
  template: `
    <div class="lang-switcher">
      <button
        type="button"
        [class.active]="activeLang() === 'tr'"
        (click)="setLang('tr')"
      >
        TR
      </button>
      <span class="lang-switcher__divider">/</span>
      <button
        type="button"
        [class.active]="activeLang() === 'en'"
        (click)="setLang('en')"
      >
        EN
      </button>
    </div>
  `,
  styles: [`
    .lang-switcher {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      font-size: 0.85rem;
    }
    .lang-switcher button {
      background: none;
      border: none;
      cursor: pointer;
      padding: 0.25rem 0.4rem;
      color: inherit;
      opacity: 0.5;
      font-weight: 500;
    }
    .lang-switcher button.active {
      opacity: 1;
      text-decoration: underline;
    }
    .lang-switcher__divider {
      opacity: 0.3;
    }
  `]
})
export class LangSwitcherComponent {
  private transloco = inject(TranslocoService);
  activeLang = toSignal(this.transloco.langChanges$, { initialValue: this.transloco.getActiveLang() });

  setLang(lang: string) {
    this.transloco.setActiveLang(lang);
    localStorage.setItem('lang', lang);
  }
}