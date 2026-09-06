import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GameService } from './game.service';
import { GameMode, GameState } from './models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  private readonly gameService = inject(GameService);

  game: GameState | null = null;
  mode: GameMode = 'TwoPlayer';
  error = '';
  busy = false;

  constructor() {
    this.startGame();
  }

  startGame(): void {
    this.error = '';
    this.gameService.createGame(this.mode).subscribe({
      next: state => this.game = state,
      error: err => this.handleError(err)
    });
  }

  changeMode(): void {
    this.startGame();
  }

  play(row: number, column: number): void {
    if (!this.game || this.game.status !== 'InProgress' ||
        this.game.board[row][column] || this.busy) return;

    this.error = '';
    this.busy = true;
    this.gameService.move(this.game.id, this.game.currentPlayer, row, column)
      .subscribe({
        next: state => {
          this.game = state;
          this.busy = false;
        },
        error: err => {
          this.busy = false;
          this.handleError(err);
        }
      });
  }

  undo(): void {
    if (!this.game || this.game.moveHistory.length === 0 ||
        this.game.status !== 'InProgress') return;

    this.error = '';
    this.gameService.undo(this.game.id).subscribe({
      next: state => this.game = state,
      error: err => this.handleError(err)
    });
  }

  resetGame(): void {
    if (!this.game) return;

    this.error = '';
    this.gameService.resetGame(this.game.id).subscribe({
      next: state => this.game = state,
      error: err => this.handleError(err)
    });
  }

  resetScoreboard(): void {
    this.gameService.resetScoreboard().subscribe({
      next: score => {
        if (this.game) this.game = { ...this.game, scoreboard: score };
      },
      error: err => this.handleError(err)
    });
  }

  isWinningCell(row: number, column: number): boolean {
    return this.game?.winningCells.some(
      p => p.row === row && p.column === column
    ) ?? false;
  }

  position(row: number, column: number): string {
    return `Row ${row + 1}, Column ${column + 1}`;
  }

  private handleError(err: { error?: { message?: string } }): void {
    this.error = err?.error?.message ?? 'Something went wrong. Please try again.';
  }
  
  row(index: number): number {
  return Math.floor(index / 3);
}

column(index: number): number {
  return index % 3;
}
}
