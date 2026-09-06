import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GameMode, GameState, Player, Scoreboard } from './models';

@Injectable({ providedIn: 'root' })
export class GameService {
  private readonly http = inject(HttpClient);

  createGame(mode: GameMode) {
    return this.http.post<GameState>('/api/games', { mode });
  }

  getGame(id: string) {
    return this.http.get<GameState>(`/api/games/${id}`);
  }

  move(id: string, player: Player, row: number, column: number) {
    return this.http.post<GameState>(`/api/games/${id}/moves`, {
      player, row, column
    });
  }

  undo(id: string) {
    return this.http.post<GameState>(`/api/games/${id}/undo`, {});
  }

  resetGame(id: string) {
    return this.http.post<GameState>(`/api/games/${id}/reset`, {});
  }

  resetScoreboard() {
    return this.http.post<Scoreboard>('/api/scoreboard/reset', {});
  }
}
