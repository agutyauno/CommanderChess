# Commander Chess - Feature List

## Tổng quan
Tài liệu này liệt kê tất cả các tính năng cần thiết cho trò chơi Commander Chess, được phân chia thành 4 phase phát triển chính cùng với hạ tầng Backend.

---

## Bảng Tóm Tắt

| Phase | Tên | Priority | Số Modules | Số Features |
|-------|-----|----------|------------|-------------|
| Phase 1 | MVP - Core Gameplay (Desktop) | Critical | 9 | ~380 |
| Phase 2 | Online & Social Features | High | 9 | ~206 |
| Phase 3 | Advanced Features & Content | Medium | 6 | ~161 |
| Phase 4 | Cross-platform & Polish | Low/Future | 6 | ~137 |
| Backend | Infrastructure (C# Custom Server) | High | 4 | ~149 |
| **Tổng cộng** | | | **34** | **~1033** |

---

## Lộ Trình Phát Triển

### Milestone Timeline

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        COMMANDER CHESS DEVELOPMENT ROADMAP                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Phase 1: MVP (3-4 months)                                                  │
│  ├── Month 1-2: Core Systems (Board, Pieces, Rules)                        │
│  ├── Month 2-3: Game Mode, Local Multiplayer, UI/UX                        │
│  └── Month 3-4: Visual Effects, Audio, Tutorial                            │
│                                                                             │
│  Phase 2: Online Features (2-3 months)                                      │
│  ├── Month 5: Authentication, Profile, Backend Setup                        │
│  ├── Month 6: Online Multiplayer, Matchmaking                              │
│  └── Month 7: Chat, Spectator, Leaderboard, Friends                        │
│                                                                             │
│  Phase 3: Advanced Features (2-3 months)                                    │
│  ├── Month 8: Replay System, Custom Setup                                   │
│  ├── Month 9: DevMode Tools, Additional Game Modes                         │
│  └── Month 10: Achievements, Cosmetics (Optional)                          │
│                                                                             │
│  Phase 4: Cross-platform (2-3 months)                                       │
│  ├── Month 11: Mobile Port (iOS & Android)                                 │
│  ├── Month 12: Web Port, Cross-platform Play                               │
│  └── Month 13: Localization, Optimization, QA                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Milestones Chi Tiết

| Milestone | Mục Tiêu | Thời Gian Dự Kiến |
|-----------|----------|-------------------|
| M1 - Alpha | Core gameplay hoàn chỉnh, local multiplayer | Tháng 2 |
| M2 - Beta | UI/UX, effects, audio, tutorial | Tháng 4 |
| M3 - Online Alpha | Authentication, online multiplayer cơ bản | Tháng 6 |
| M4 - Online Beta | Full social features | Tháng 7 |
| M5 - Advanced | Replay, custom setup, dev tools | Tháng 9 |
| M6 - RC | Mobile & Web ports | Tháng 12 |
| M7 - Release | Full localization, optimization | Tháng 13 |

---

## Phase 1: MVP - Core Gameplay (Desktop)
**Priority: Critical**

### Module 1: Game Board System

#### 1.1 Board Initialization
- [ ] Tạo bàn cờ 12x11 (VA 0-11, HA 0-10)
- [ ] Thiết lập hệ tọa độ decimal base
- [ ] Origin point (0,0) tại góc dưới trái (vùng biển xanh)
- [ ] Đọc tọa độ theo quy tắc: VA trước, HA sau (vd: 3,5)

#### 1.2 Terrain System
- [ ] Land terrain (đất liền)
- [ ] Sea terrain (biển sâu)
- [ ] River terrain (sông)
  - [ ] Deep water segments (nước sâu)
  - [ ] Shallow/Reef-base segments (bãi cạn - 2 ô)
- [ ] Headquarters positions (2 bunker mỗi bên)
- [ ] Terrain data structure và storage

#### 1.3 Visual Board Rendering
- [ ] Board grid rendering
- [ ] Terrain visual differentiation
- [ ] Coordinate labels display (VA, HA numbers)
- [ ] Board orientation (Red bottom, Blue top)

#### 1.4 Highlight System
- [ ] Valid move highlights (nơi có thể đi)
- [ ] Attack range highlights (nơi có thể tấn công)
- [ ] Selected piece highlight
- [ ] Last move indicator
- [ ] Check/Checkmate warning highlight
- [ ] Ring of fire visualization
  - [ ] Small ring (1 segment - Anti-aircraft)
  - [ ] Large ring (2 segments - Missile)
- [ ] Terrain restriction highlights

#### 1.5 Board Interaction
- [ ] Click to select piece
- [ ] Click to move/attack
- [ ] Drag and drop support
- [ ] Right-click to cancel selection
- [ ] Touch input preparation (for future mobile)

---

### Module 2: Piece System

#### 2.1 Base Piece Structure
- [ ] Piece base class/interface
- [ ] Piece identification (ID, type, team)
- [ ] Position tracking
- [ ] State management (normal, heroic, carried, destroyed)
- [ ] Point value system

#### 2.2 Commander (C) - 100 điểm
- [ ] Di chuyển tự do theo VA và HA (không giới hạn ô)
- [ ] Bắt quân trong phạm vi 1 ô
- [ ] Không di chuyển chéo
- [ ] Có thể vào Headquarter
- [ ] Không được đối mặt trực tiếp với Commander đối phương
- [ ] Có thể bỏ qua biển khi lên Navy
- [ ] Có thể được mang bởi Tank, Air Force, Navy

#### 2.3 Infantry (In) x2 - 10 điểm mỗi quân
- [ ] Di chuyển và bắt quân 1 ô theo 4 hướng (trên, dưới, trái, phải)
- [ ] Qua sông tự do (cả nước sâu và bãi cạn)
- [ ] Có thể được mang bởi Tank, Air Force, Navy
- [ ] Heroic: di chuyển và bắt 1-2 ô, thêm hướng chéo

#### 2.4 Tank (T) x2 - 20 điểm mỗi quân
- [ ] Di chuyển và bắt quân 1-2 ô theo VA và HA
- [ ] Tấn công quân ở biển: có thể đứng yên bắn (không cần chiếm vị trí)
- [ ] Qua sông tự do
- [ ] Có thể mang 2 quân (Commander, Infantry, Militia)
- [ ] Heroic: di chuyển và bắt 1-3 ô

#### 2.5 Militia (M) x1 - 10 điểm
- [ ] Di chuyển tất cả hướng (8 hướng), 1 ô
- [ ] Qua sông tự do
- [ ] Có thể được mang bởi Tank, Air Force, Navy
- [ ] Heroic: di chuyển và bắt 1-2 ô

#### 2.6 Engineer (E) x2 - 10 điểm mỗi quân
- [ ] Di chuyển và bắt quân 1 ô theo 4 hướng
- [ ] Có thể mang quân nặng qua sông (Artillery, Anti-aircraft, Missile)
- [ ] Qua sông tự do

#### 2.7 Artillery (A) x2 - 30 điểm mỗi quân
- [ ] Di chuyển 1-3 ô theo tất cả hướng (8 hướng)
- [ ] Qua sông: phải qua bãi cạn (khi chỉ di chuyển)
- [ ] Qua sông tự do khi bắt quân ở bên kia sông
- [ ] Bắt quân trên đất: phải chiếm vị trí
- [ ] Bắt quân ở biển: đứng yên bắn (1-3 ô)
- [ ] Cần Engineer để qua nước sâu (khi không bắt quân)
- [ ] Heroic: di chuyển và bắt 1-4 ô

#### 2.8 Anti-aircraft (Aa) x2 - 10 điểm mỗi quân
- [ ] Di chuyển và bắt quân 1 ô theo 4 hướng
- [ ] Ring of Fire: bán kính 1 ô (bảo vệ khỏi Air Force)
- [ ] Qua sông: phải qua bãi cạn hoặc được Engineer mang
- [ ] Cần Engineer để qua nước sâu
- [ ] Heroic: di chuyển và bắt 1-2 ô, thêm hướng chéo

#### 2.9 Missile (Ms) x1 - 20 điểm
- [ ] Di chuyển và bắt quân trong vòng lửa bán kính 2 ô
  - [ ] 2 ô theo VA và HA
  - [ ] 1 ô theo đường chéo (góc 45°)
- [ ] Ring of Fire: bán kính 2 ô (lớn)
- [ ] Có thể bắt quân trên mặt đất và trên không
- [ ] Cần Engineer để qua nước sâu
- [ ] Heroic: bán kính di chuyển +1

#### 2.10 Air Force (Af) x2 - 40 điểm mỗi quân
- [ ] Di chuyển và bắt quân 1-4 ô theo tất cả hướng (8 hướng)
- [ ] Bay qua mọi chướng ngại vật
- [ ] Phải thay thế vị trí khi bắt quân
- [ ] Exception: tấn công quân đất, nếu vị trí không an toàn → quay về vị trí ban đầu
- [ ] Bay qua ring of fire → bị bắn hạ ngay lập tức
- [ ] Tấn công quân trong ring of fire → đổi 1-1 (cả hai bị tiêu diệt)
- [ ] Khi được Navy mang → không bị ảnh hưởng bởi ring of fire
- [ ] Không thể kết thúc di chuyển ở biển
- [ ] Có thể mang 2 quân (Commander, Infantry, Militia)
- [ ] Heroic: trở thành Stealth Air Force
  - [ ] Bắn Anti-aircraft và Missile mà không bị crash
  - [ ] Bỏ qua ring of fire

#### 2.11 Navy (N) x2 - 80 điểm mỗi quân
- [ ] Di chuyển 1-4 ô theo tất cả hướng (8 hướng)
- [ ] Di chuyển vào sông nước sâu (không qua bãi cạn)
- [ ] Không bị chặn bởi quân cùng phe khi di chuyển
- [ ] Trang bị:
  - [ ] Anti-aircraft gun: ring of fire bán kính 1 ô (10 điểm)
  - [ ] Gunboat: bắn như Artillery (1-3 ô, 30 điểm)
  - [ ] Anti-ship missile: chỉ bắn Navy đối phương (1-4 ô, 40 điểm)
- [ ] Gunboat bắn đất liền: có thể đứng yên bắn
- [ ] Gunboat bắt quân ven biển: phải chiếm vị trí
- [ ] Anti-ship missile bắt Navy: phải chiếm vị trí
- [ ] Có đất liền giữa Navy và mục tiêu: đứng yên bắn
- [ ] Có thể mang 2 quân (bao gồm cả Air Force)
- [ ] Heroic: di chuyển và bắt 1-5 ô

#### 2.12 Headquarter (H) x2 - 0 điểm (không thể bắt)
- [ ] Không thể di chuyển
- [ ] Không thể tấn công
- [ ] Chỉ Commander có thể vào
- [ ] Có thể bị phá hủy để bắt Commander bên trong
- [ ] Heroic (khi là quân cuối bảo vệ Commander):
  - [ ] Có thể di chuyển và bắt như Heroic Infantry (1-2 ô, 8 hướng)

#### 2.13 Piece Visual & Animation
- [ ] 2D sprite cho mỗi loại quân
- [ ] Red team variants
- [ ] Blue team variants
- [ ] Heroic visual indicator (vàng/sáng)
- [ ] Carried piece indicator
- [ ] Selection animation
- [ ] Move animation
- [ ] Attack animation
- [ ] Destroy animation
- [ ] Spawn animation

---

### Module 3: Core Game Rules Implementation

#### 3.1 Turn System
- [ ] Turn alternation (Red first, then Blue)
- [ ] Turn timer (configurable)
- [ ] Turn indicator UI
- [ ] Auto-end turn on action complete
- [ ] Skip turn option (for combined piece operations)

#### 3.2 Movement Validation
- [ ] Valid move calculation cho mỗi loại quân
- [ ] Terrain restriction check
- [ ] Path blocking check (cho quân không bay)
- [ ] Ring of fire danger check
- [ ] Commander face-to-face restriction
- [ ] River crossing rules validation
  - [ ] Shallow crossing (tất cả quân)
  - [ ] Deep water crossing (Commander, Infantry, Militia, Tank, Engineer)
  - [ ] Heavy piece crossing (cần Engineer hoặc bắt quân)

#### 3.3 Capture System
- [ ] Standard capture (thay thế vị trí)
- [ ] Ranged capture without replacement
  - [ ] Tank → Sea targets
  - [ ] Artillery → Sea targets
  - [ ] Navy gunboat → Land targets (có đất liền giữa)
- [ ] Air Force special capture rules
- [ ] Score calculation on capture
- [ ] Capture history logging

#### 3.4 Heroic System
- [ ] Check detection (khi đe dọa Commander đối phương)
- [ ] Heroic promotion trigger
- [ ] Heroic stat modification
  - [ ] +1 segment di chuyển
  - [ ] Thêm hướng chéo (cho quân 4 hướng)
- [ ] Air Force → Stealth Air Force transformation
- [ ] Last defender heroic promotion
- [ ] Headquarter heroic transformation
- [ ] Heroic visual feedback

#### 3.5 Ring of Fire System
- [ ] Small ring calculation (Anti-aircraft, Navy AA gun)
- [ ] Large ring calculation (Missile)
- [ ] Air Force collision detection with rings
- [ ] Auto-destroy Air Force entering ring
- [ ] Mutual destruction when attacking into ring
- [ ] Ring visualization toggle
- [ ] Navy-carried pieces immunity

#### 3.6 Terrific Speed Operation (Combined Pieces)
- [ ] Boarding mechanics
  - [ ] Infantry/Militia/Commander → Tank
  - [ ] Infantry/Militia/Commander → Air Force
  - [ ] Any valid piece → Navy
- [ ] Skip turn for boarding
- [ ] Combined piece movement
- [ ] Multi-direction split (lên đến 3 quân, 3 hướng)
- [ ] Multi-capture in single turn (lên đến 3 quân)
- [ ] Split from same point rule
- [ ] Capacity limit (2 quân được mang)
- [ ] Combined piece destruction scoring

#### 3.7 Check & Checkmate
- [ ] Check detection
- [ ] Checkmate detection
- [ ] Check notification
- [ ] Checkmate notification
- [ ] Force move out of check
- [ ] Air Force can check Commander in ring (with conditions)

#### 3.8 Win Conditions
- [ ] Commander captured → Instant win
- [ ] Score-based win (time out)
- [ ] Tactical victory conditions
  - [ ] Marine battle: 2 Navy destroyed
  - [ ] Air battle: 2 Air Force destroyed
  - [ ] Land battle: 2 Tank + 2 Infantry + 2 Artillery destroyed
  - [ ] Raid battle: Commander destroyed
- [ ] Victory bonus: +50 points

---

### Module 4: Game Mode - Standard (Total Force 30 phút)

#### 4.1 Mode Configuration
- [ ] Time limit: 30 minutes
- [ ] Full piece setup (19 quân mỗi bên)
- [ ] Standard board layout
- [ ] Win condition: Commander captured hoặc score tại time out

#### 4.2 Timer System
- [ ] Total game timer (30 phút)
- [ ] Timer display (MM:SS format)
- [ ] Low time warning (< 5 phút)
- [ ] Critical time warning (< 1 phút)
- [ ] Time out handling

#### 4.3 Score Tracking
- [ ] Real-time score display (cả 2 bên)
- [ ] Score breakdown popup
- [ ] Score history trong game
- [ ] Final score calculation

#### 4.4 Initial Setup
- [ ] Standard piece placement
- [ ] Red side setup (bottom)
- [ ] Blue side setup (top)
- [ ] Setup validation

---

### Module 5: Local Multiplayer (Hot-seat)

#### 5.1 Player Management
- [ ] Player 1 (Red) setup
- [ ] Player 2 (Blue) setup
- [ ] Player name input
- [ ] Player avatar selection (basic)

#### 5.2 Turn Handling
- [ ] Turn switch notification
- [ ] Board rotation option (flip view for current player)
- [ ] Clear turn indication

#### 5.3 Game Flow
- [ ] New game setup
- [ ] In-game pause
- [ ] Resign option
- [ ] Rematch option
- [ ] Return to menu

#### 5.4 Local Save/Load
- [ ] Quick save
- [ ] Manual save
- [ ] Save slot system (3-5 slots)
- [ ] Load game
- [ ] Auto-save

---

### Module 6: UI/UX Core

#### 6.1 Main Menu
- [ ] Start New Game button
- [ ] Continue Game button
- [ ] Settings button
- [ ] Tutorial button
- [ ] Quit button
- [ ] Background art/animation
- [ ] Menu music

#### 6.2 Game Mode Selection
- [ ] Mode selection screen
- [ ] Mode description
- [ ] Time setting display
- [ ] Player count indicator
- [ ] Start button

#### 6.3 In-Game HUD
- [ ] Score display (both players)
- [ ] Timer display
- [ ] Current turn indicator
- [ ] Selected piece info panel
- [ ] Captured pieces display
- [ ] Action buttons (Undo, Settings, etc.)

#### 6.4 Score Display
- [ ] Player 1 score (Red)
- [ ] Player 2 score (Blue)
- [ ] Score difference indicator
- [ ] Animated score change
- [ ] Score breakdown on hover/click

#### 6.5 Timer Display
- [ ] Game timer (MM:SS)
- [ ] Turn timer (if applicable)
- [ ] Visual warning on low time
- [ ] Sound warning on low time

#### 6.6 Confirm/Cancel System
- [ ] Move confirmation prompt (optional setting)
- [ ] Cancel current selection
- [ ] Undo last move (optional - before opponent moves)
- [ ] Action confirmation dialogs

#### 6.7 Settings Menu
- [ ] Sound volume controls
  - [ ] Master volume
  - [ ] Music volume
  - [ ] SFX volume
- [ ] Display settings
  - [ ] Fullscreen toggle
  - [ ] Resolution selection
  - [ ] Quality settings
- [ ] Gameplay settings
  - [ ] Move confirmation toggle
  - [ ] Highlight toggle
  - [ ] Animation speed
- [ ] Control settings
  - [ ] Key rebinding (basic)

#### 6.8 Pause Menu
- [ ] Resume button
- [ ] Settings button
- [ ] Save Game button
- [ ] Main Menu button (with confirmation)
- [ ] Quit button (with confirmation)

#### 6.9 Game Over Screen
- [ ] Winner announcement
- [ ] Final score display
- [ ] Score breakdown
- [ ] Rematch button
- [ ] Main Menu button

#### 6.10 Popup & Dialog System
- [ ] Generic popup template
- [ ] Confirmation dialog
- [ ] Info dialog
- [ ] Error dialog
- [ ] Loading indicator

---

### Module 7: Visual Effects (2D)

#### 7.1 Piece Animations
- [ ] Idle animation
- [ ] Selection highlight animation
- [ ] Move animation (smooth transition)
- [ ] Jump animation (for river crossing)
- [ ] Attack animation
- [ ] Capture animation
- [ ] Death/Destroy animation
- [ ] Heroic promotion animation

#### 7.2 Board Effects
- [ ] Tile highlight effects
- [ ] Valid move particle effects
- [ ] Attack range particle effects
- [ ] Ring of fire visual effect
- [ ] Check warning effect
- [ ] Checkmate effect

#### 7.3 Combat Effects
- [ ] Melee attack effect
- [ ] Ranged attack effect
- [ ] Explosion effect (for destroyed pieces)
- [ ] Air Force crash effect
- [ ] Navy cannon fire effect
- [ ] Missile launch effect

#### 7.4 UI Effects
- [ ] Button hover effects
- [ ] Button click effects
- [ ] Score change animation
- [ ] Timer warning pulse
- [ ] Turn change transition
- [ ] Victory celebration effect

#### 7.5 Terrain Effects
- [ ] Water ripple animation
- [ ] River flow animation
- [ ] Land texture variation
- [ ] Sea wave animation

---

### Module 8: Audio System

#### 8.1 Background Music
- [ ] Main menu theme
- [ ] In-game music (ambient/strategic)
- [ ] Tension music (check/low time)
- [ ] Victory music
- [ ] Defeat music
- [ ] Music crossfade system
- [ ] Music loop system

#### 8.2 Sound Effects - Pieces
- [ ] Piece selection sound
- [ ] Piece deselection sound
- [ ] Move sound (per terrain type)
- [ ] Capture sound
- [ ] Check sound
- [ ] Checkmate sound
- [ ] Heroic promotion sound

#### 8.3 Sound Effects - Combat
- [ ] Melee attack sound
- [ ] Ranged attack sound
- [ ] Artillery fire sound
- [ ] Air Force flyby sound
- [ ] Navy cannon sound
- [ ] Missile launch sound
- [ ] Explosion sound
- [ ] Ring of fire activation sound

#### 8.4 Sound Effects - UI
- [ ] Button click sound
- [ ] Button hover sound
- [ ] Menu open/close sound
- [ ] Timer tick sound (low time)
- [ ] Score change sound
- [ ] Error/Invalid action sound
- [ ] Confirmation sound
- [ ] Turn change sound

#### 8.5 Audio Settings
- [ ] Master volume control
- [ ] Music volume control
- [ ] SFX volume control
- [ ] Mute toggle
- [ ] Audio mixer system

---

### Module 9: Tutorial System

#### 9.1 Interactive Tutorial
- [ ] Tutorial flow controller
- [ ] Step-by-step guidance
- [ ] Highlight target elements
- [ ] Dimming non-target areas
- [ ] Progress tracking
- [ ] Skip tutorial option

#### 9.2 Tutorial Content - Basics
- [ ] Introduction to Commander Chess
- [ ] Board layout explanation
- [ ] Terrain types explanation
- [ ] Coordinate system tutorial
- [ ] Basic UI overview

#### 9.3 Tutorial Content - Pieces
- [ ] Commander tutorial
- [ ] Infantry tutorial
- [ ] Tank tutorial
- [ ] Militia tutorial
- [ ] Engineer tutorial
- [ ] Artillery tutorial
- [ ] Anti-aircraft tutorial
- [ ] Missile tutorial
- [ ] Air Force tutorial
- [ ] Navy tutorial
- [ ] Headquarter tutorial

#### 9.4 Tutorial Content - Rules
- [ ] Movement rules tutorial
- [ ] Capture rules tutorial
- [ ] River crossing tutorial
- [ ] Ring of fire tutorial
- [ ] Heroic system tutorial
- [ ] Combined pieces tutorial
- [ ] Check & Checkmate tutorial
- [ ] Win conditions tutorial
- [ ] Score system tutorial

#### 9.5 Practice Mode
- [ ] Sandbox mode (tự do thử nghiệm)
- [ ] Reset board option
- [ ] Place any piece option
- [ ] Remove piece option
- [ ] Switch sides option
- [ ] No win condition (endless practice)

#### 9.6 Tutorial Tips
- [ ] Contextual tooltips
- [ ] First-time user hints
- [ ] Strategy tips (optional)
- [ ] "Did you know?" popups

---

## Phase 2: Online & Social Features
**Priority: High**

### Module 10: Authentication System

#### 10.1 Email/Password Authentication
- [ ] Registration with email
- [ ] Email verification
- [ ] Login with email/password
- [ ] Password requirements validation
- [ ] Remember me option
- [ ] Logout functionality

#### 10.2 Password Management
- [ ] Forgot password flow
- [ ] Password reset email
- [ ] Change password (in settings)
- [ ] Password strength indicator

#### 10.3 Session Management
- [ ] JWT token management
- [ ] Token refresh mechanism
- [ ] Auto-logout on token expiry
- [ ] Multi-device session handling
- [ ] Session timeout settings

#### 10.4 Security
- [ ] Password hashing (bcrypt)
- [ ] Rate limiting for auth endpoints
- [ ] Account lockout after failed attempts
- [ ] Secure token storage
- [ ] HTTPS enforcement

---

### Module 11: Player Profile System

#### 11.1 Profile Information
- [ ] Display name
- [ ] Avatar selection (pre-defined)
- [ ] Avatar upload (optional - future)
- [ ] Player ID (unique)
- [ ] Join date
- [ ] Last online status

#### 11.2 Statistics
- [ ] Total games played
- [ ] Wins count
- [ ] Losses count
- [ ] Win rate percentage
- [ ] Win streak (current/best)
- [ ] Total play time
- [ ] Favorite piece (most used for captures)

#### 11.3 ELO Rating System
- [ ] Initial ELO (1200 default)
- [ ] ELO calculation after each game
- [ ] ELO history tracking
- [ ] Rank titles based on ELO
  - [ ] Bronze: < 1200
  - [ ] Silver: 1200-1400
  - [ ] Gold: 1400-1600
  - [ ] Platinum: 1600-1800
  - [ ] Diamond: 1800-2000
  - [ ] Master: 2000-2200
  - [ ] Grandmaster: > 2200
- [ ] Rank icon display

#### 11.4 Match History
- [ ] Recent matches list
- [ ] Match details (opponent, result, score, duration)
- [ ] Match replay link (Phase 3)
- [ ] Filter by result (win/loss)
- [ ] Filter by date range
- [ ] Pagination

#### 11.5 Profile Settings
- [ ] Edit display name
- [ ] Change avatar
- [ ] Privacy settings
- [ ] Notification settings
- [ ] Account deletion request

---

### Module 12: Online Multiplayer - Room System

#### 12.1 Room Creation
- [ ] Create room button
- [ ] Room name input
- [ ] Room password (optional)
- [ ] Game mode selection
- [ ] Time limit selection
- [ ] Room visibility (public/private)
- [ ] Max spectators setting

#### 12.2 Room Management
- [ ] Room host controls
- [ ] Kick player option
- [ ] Change settings (before game start)
- [ ] Close room option
- [ ] Room auto-close on empty

#### 12.3 Room Lobby
- [ ] Room code display
- [ ] Share room link
- [ ] Player slots (2 players)
- [ ] Ready status display
- [ ] Chat in lobby
- [ ] Start game button (host only)
- [ ] Leave room button

#### 12.4 Room Browser
- [ ] Public room list
- [ ] Search by room name
- [ ] Filter by game mode
- [ ] Filter by ELO range
- [ ] Join room button
- [ ] Room status indicator (waiting/in-progress)
- [ ] Refresh button
- [ ] Pagination

#### 12.5 Room Invitation
- [ ] Invite friend (from friend list)
- [ ] Invite by player ID
- [ ] Copy room link
- [ ] Invitation notification
- [ ] Accept/Decline invitation

---

### Module 13: Online Multiplayer - Matchmaking

#### 13.1 Matchmaking Queue
- [ ] Join matchmaking button
- [ ] Queue status display
- [ ] Estimated wait time
- [ ] Cancel matchmaking button
- [ ] Queue position (optional)

#### 13.2 ELO-based Matching
- [ ] Initial ELO range (±100)
- [ ] Expanding ELO range over time
- [ ] Maximum ELO difference cap
- [ ] Prefer similar game count (for new players)

#### 13.3 Match Found
- [ ] Match found notification
- [ ] Accept/Decline prompt
- [ ] Ready check (both players)
- [ ] Timeout handling (auto-decline)
- [ ] Return to queue on decline

#### 13.4 Game Mode Selection
- [ ] Mode filter for matchmaking
- [ ] Preferred time limit
- [ ] Queue for multiple modes (optional)

---

### Module 14: Online Gameplay

#### 14.1 Network Synchronization
- [ ] Game state synchronization
- [ ] Move validation on server
- [ ] Client-side prediction
- [ ] Lag compensation
- [ ] Conflict resolution

#### 14.2 Connection Handling
- [ ] Connection status indicator
- [ ] Latency display (ping)
- [ ] Connection quality warning
- [ ] Graceful degradation

#### 14.3 Reconnection System
- [ ] Auto-reconnect on disconnect
- [ ] Reconnection timeout (2 minutes)
- [ ] Game state recovery
- [ ] Opponent notification on disconnect
- [ ] Manual reconnect option

#### 14.4 AFK Detection
- [ ] Inactivity timer
- [ ] AFK warning notification
- [ ] Auto-resign on extended AFK
- [ ] Report AFK player option
- [ ] Grace period for reconnection

#### 14.5 Game Result Handling
- [ ] Result validation on server
- [ ] ELO update on game end
- [ ] Match history recording
- [ ] Disconnect/forfeit handling
- [ ] Cheating detection (basic)

---

### Module 15: Chat System

#### 15.1 In-Game Chat
- [ ] Text input field
- [ ] Send message button
- [ ] Message history display
- [ ] Timestamps on messages
- [ ] Scroll through history

#### 15.2 Quick Messages
- [ ] Pre-defined quick messages
- [ ] "Good game" 
- [ ] "Good luck"
- [ ] "Nice move"
- [ ] "Thanks"
- [ ] "Thinking..."
- [ ] Custom quick message shortcuts

#### 15.3 Chat Moderation
- [ ] Profanity filter
- [ ] Spam prevention (rate limiting)
- [ ] Mute opponent option
- [ ] Report message option
- [ ] Block player option

#### 15.4 Chat Settings
- [ ] Enable/Disable chat
- [ ] Chat sound notifications
- [ ] Quick messages only mode
- [ ] Font size options

---

### Module 16: Spectator System

#### 16.1 Spectating Interface
- [ ] Spectate button (from room/match)
- [ ] Spectator view (sees both sides)
- [ ] Hidden information protection (nếu có)
- [ ] Spectator count display

#### 16.2 Real-time Updates
- [ ] Live game state updates
- [ ] Move notifications
- [ ] Score updates
- [ ] Timer synchronization
- [ ] Minimal delay

#### 16.3 Spectator Features
- [ ] Board zoom
- [ ] Move history panel
- [ ] Player info display
- [ ] Game statistics display
- [ ] Leave spectating button

#### 16.4 Spectator Management
- [ ] No spectator limit
- [ ] Spectator list (for host)
- [ ] Kick spectator (optional)
- [ ] Spectator chat (separate from player chat)

---

### Module 17: Leaderboard System

#### 17.1 Global Leaderboard
- [ ] Top players list (top 100)
- [ ] ELO-based ranking
- [ ] Player name & rank display
- [ ] Win/Loss ratio display
- [ ] Search player in leaderboard

#### 17.2 Leaderboard Views
- [ ] All-time leaderboard
- [ ] Monthly leaderboard
- [ ] Weekly leaderboard
- [ ] Daily leaderboard
- [ ] By game mode (optional)

#### 17.3 Personal Ranking
- [ ] My rank display
- [ ] Players around my rank
- [ ] Progress to next rank
- [ ] Rank change indicator

#### 17.4 Leaderboard Features
- [ ] Pagination
- [ ] Click to view profile
- [ ] Challenge player (optional)
- [ ] Refresh button
- [ ] Last updated timestamp

---

### Module 18: Friend System

#### 18.1 Friend Management
- [ ] Friend list display
- [ ] Online/Offline status
- [ ] Currently in-game status
- [ ] Sort by status/name
- [ ] Search friends

#### 18.2 Friend Requests
- [ ] Send friend request
- [ ] Search player by name/ID
- [ ] Pending requests list
- [ ] Accept/Decline request
- [ ] Request notification

#### 18.3 Friend Actions
- [ ] View friend profile
- [ ] Invite to game
- [ ] Spectate friend's game
- [ ] Remove friend
- [ ] Block player

#### 18.4 Friend Notifications
- [ ] Friend online notification
- [ ] Friend request notification
- [ ] Game invitation notification
- [ ] Notification settings

---

## Phase 3: Advanced Features & Content
**Priority: Medium**

### Module 19: Replay System

#### 19.1 Replay Recording
- [ ] Automatic game recording
- [ ] Move-by-move data storage
- [ ] Timestamps for each move
- [ ] Game metadata (players, scores, duration)
- [ ] Efficient data compression

#### 19.2 Replay Playback
- [ ] Play/Pause button
- [ ] Forward one move
- [ ] Backward one move
- [ ] Fast forward (2x, 4x speed)
- [ ] Rewind (2x, 4x speed)
- [ ] Jump to specific move
- [ ] Move slider/timeline
- [ ] Jump to start/end

#### 19.3 Replay Navigation
- [ ] Move list panel
- [ ] Click move to jump
- [ ] Move annotations display
- [ ] Board state at any point
- [ ] Score at any point

#### 19.4 Replay Features
- [ ] Save replay locally
- [ ] Share replay link
- [ ] Download replay file
- [ ] Replay in spectator mode
- [ ] Add personal notes (optional)

#### 19.5 Replay Browser
- [ ] My replays list
- [ ] Public replays (featured)
- [ ] Friend replays
- [ ] Search/Filter replays
- [ ] Sort by date/rating
- [ ] Delete replay

---

### Module 20: Custom Setup System

#### 20.1 Setup Editor Interface
- [ ] Empty board display
- [ ] Piece palette
- [ ] Drag piece to board
- [ ] Remove piece from board
- [ ] Clear board option
- [ ] Reset to default setup

#### 20.2 Setup Rules
- [ ] Piece count limits
- [ ] Valid placement zones
- [ ] Commander required
- [ ] Minimum piece count
- [ ] Setup validation

#### 20.3 Preset Management
- [ ] Save preset
- [ ] Preset name input
- [ ] Load preset
- [ ] Delete preset
- [ ] Preset list display
- [ ] Default presets (standard setup)

#### 20.4 Custom Game Start
- [ ] Use custom setup in local game
- [ ] Use custom setup in private room
- [ ] Share preset with opponent
- [ ] Both players agree to custom setup

---

### Module 21: DevMode - Game Designer Tools

#### 21.1 Piece Stats Editor
- [ ] Select piece type
- [ ] Edit movement range
- [ ] Edit attack range
- [ ] Edit point value
- [ ] Edit special abilities
- [ ] Preview changes
- [ ] Save/Load stat presets
- [ ] Reset to default stats

#### 21.2 Board Editor
- [ ] Edit board dimensions
- [ ] Edit terrain types per cell
- [ ] Place/Remove headquarters
- [ ] Edit river configuration
- [ ] Save/Load board layouts
- [ ] Export board as file

#### 21.3 Debug Tools
- [ ] Enable debug mode toggle
- [ ] Show all valid moves
- [ ] Show ring of fire ranges
- [ ] Skip to any turn
- [ ] Force piece placement
- [ ] Force score change
- [ ] Log viewer
- [ ] Performance stats display

#### 21.4 Testing Utilities
- [ ] AI opponent (basic)
- [ ] Auto-play mode
- [ ] Random move generator
- [ ] Stress test mode
- [ ] Bug report generator

#### 21.5 DevMode Access
- [ ] DevMode unlock (password/key)
- [ ] DevMode menu
- [ ] Exit DevMode
- [ ] DevMode indicator on screen

---

### Module 22: Additional Game Modes

#### 22.1 Tactical Mode (15 phút)
- [ ] Time limit: 15 minutes
- [ ] Victory conditions:
  - [ ] Marine battle: Destroy 2 Navy
  - [ ] Air battle: Destroy 2 Air Force
  - [ ] Land battle: Destroy 2 Tank + 2 Infantry + 2 Artillery
  - [ ] Raid battle: Destroy Commander
- [ ] Tactical victory bonus: +50 points
- [ ] Score-based win at timeout

#### 22.2 Sea Battle Mode (9 quân, 15 phút)
- [ ] Reduced piece set (9 pieces per side)
- [ ] Sea-focused piece selection
  - [ ] 2 Navy
  - [ ] 2 Air Force
  - [ ] 1 Commander
  - [ ] 2 Infantry
  - [ ] 2 Anti-aircraft
- [ ] Victory: Destroy 2 Navy first
- [ ] Time limit: 15 minutes
- [ ] Special board (more sea area)

#### 22.3 Air Battle Mode (9 quân, 15 phút)
- [ ] Reduced piece set (9 pieces per side)
- [ ] Air-focused piece selection
  - [ ] 2 Air Force
  - [ ] 2 Anti-aircraft
  - [ ] 1 Missile
  - [ ] 1 Commander
  - [ ] 2 Infantry
  - [ ] 1 Navy
- [ ] Victory: Destroy 2 Air Force first
- [ ] Time limit: 15 minutes
- [ ] Special board (more open terrain)

#### 22.4 Mode Selection UI
- [ ] Mode carousel/list
- [ ] Mode description
- [ ] Mode-specific rules display
- [ ] Piece count display
- [ ] Estimated game duration

---

### Module 23: Achievements & Progression (Optional - Future)

#### 23.1 Achievement System
- [ ] Achievement definition structure
- [ ] Achievement unlock detection
- [ ] Achievement notification popup
- [ ] Achievement icon/badge
- [ ] Rarity levels (common, rare, epic, legendary)

#### 23.2 Achievement Categories
- [ ] Win achievements
  - [ ] First win
  - [ ] 10/50/100/500 wins
  - [ ] Win streak (3/5/10)
- [ ] Gameplay achievements
  - [ ] Heroic piece promotion
  - [ ] Multi-capture with combined piece
  - [ ] Air Force stealth kill
- [ ] Social achievements
  - [ ] First friend
  - [ ] 10/50/100 friends
  - [ ] Watch replay
- [ ] Milestone achievements
  - [ ] First Commander captured
  - [ ] 100 pieces captured
  - [ ] Play 1000 games

#### 23.3 Achievement Display
- [ ] Achievements tab in profile
- [ ] Locked/Unlocked status
- [ ] Progress tracking (X/Y)
- [ ] Achievement details popup
- [ ] Share achievement

#### 23.4 Progression System
- [ ] Player level
- [ ] XP system
- [ ] Level-up rewards
- [ ] Level display on profile

---

### Module 24: Cosmetic Customization (Optional - Future)

#### 24.1 Piece Skins
- [ ] Alternative piece art styles
- [ ] Skin preview
- [ ] Skin selection per piece type
- [ ] Skin sets (themed collections)

#### 24.2 Board Themes
- [ ] Alternative board designs
- [ ] Terrain texture variations
- [ ] Board theme preview
- [ ] Theme selection

#### 24.3 Avatar Customization
- [ ] More avatar options
- [ ] Avatar frames
- [ ] Custom avatar upload
- [ ] Avatar preview

#### 24.4 Cosmetic Acquisition
- [ ] Unlock via achievements
- [ ] Unlock via progression
- [ ] In-game currency (optional)
- [ ] Store interface (optional)

---

## Phase 4: Cross-platform & Polish
**Priority: Low/Future**

### Module 25: Mobile Port (iOS & Android)

#### 25.1 Touch Controls
- [ ] Tap to select piece
- [ ] Tap to move/attack
- [ ] Drag and drop (optional)
- [ ] Pinch to zoom
- [ ] Pan to scroll board
- [ ] Multi-touch handling
- [ ] Touch feedback (haptic)

#### 25.2 Mobile UI Adaptation
- [ ] Responsive layout
- [ ] Portrait/Landscape support
- [ ] Larger touch targets
- [ ] Mobile-friendly menus
- [ ] Swipe gestures for navigation
- [ ] On-screen buttons optimization

#### 25.3 iOS Specific
- [ ] iOS build configuration
- [ ] App Store submission preparation
- [ ] Game Center integration (optional)
- [ ] iOS notifications
- [ ] iPhone & iPad support
- [ ] iOS-specific UI guidelines

#### 25.4 Android Specific
- [ ] Android build configuration
- [ ] Google Play submission preparation
- [ ] Google Play Games integration (optional)
- [ ] Android notifications
- [ ] Multiple screen size support
- [ ] Android back button handling

#### 25.5 Mobile Performance
- [ ] Mobile-optimized assets
- [ ] Reduced effects for mobile
- [ ] Battery usage optimization
- [ ] Network optimization for mobile
- [ ] Offline mode support

---

### Module 26: Web Port (WebGL)

#### 26.1 WebGL Build
- [ ] Unity WebGL build configuration
- [ ] Loading screen
- [ ] Browser compatibility testing
  - [ ] Chrome
  - [ ] Firefox
  - [ ] Safari
  - [ ] Edge
- [ ] Responsive canvas

#### 26.2 Web-specific Features
- [ ] Keyboard controls
- [ ] Mouse controls
- [ ] Browser fullscreen mode
- [ ] URL-based game sharing
- [ ] Web notifications (optional)

#### 26.3 Web Optimization
- [ ] Asset streaming
- [ ] Compressed textures
- [ ] Lazy loading
- [ ] Caching strategies
- [ ] Minimal initial download

#### 26.4 Web Hosting
- [ ] Static hosting setup
- [ ] CDN configuration
- [ ] HTTPS setup
- [ ] Domain configuration
- [ ] Error page handling

---

### Module 27: Cross-platform Play

#### 27.1 Unified Account
- [ ] Same account across platforms
- [ ] Progress synchronization
- [ ] Profile sync
- [ ] Friend list sync
- [ ] Settings sync

#### 27.2 Cross-platform Matchmaking
- [ ] Platform-agnostic matchmaking
- [ ] Platform indicator in game
- [ ] Fair play considerations
- [ ] Platform preference option

#### 27.3 Cross-platform Features
- [ ] Chat across platforms
- [ ] Friend system across platforms
- [ ] Replay sharing across platforms
- [ ] Leaderboard unified

---

### Module 28: Localization

#### 28.1 Vietnamese Localization
- [ ] UI text translation
- [ ] Tutorial translation
- [ ] In-game messages translation
- [ ] System messages translation
- [ ] Audio narration (optional)

#### 28.2 English Localization
- [ ] UI text translation
- [ ] Tutorial translation
- [ ] In-game messages translation
- [ ] System messages translation
- [ ] Grammar & spelling check

#### 28.3 Localization System
- [ ] Localization file format (JSON/CSV)
- [ ] Language selection in settings
- [ ] Auto-detect system language
- [ ] Fallback language (English)
- [ ] Dynamic text sizing for different languages
- [ ] RTL support preparation (future)

#### 28.4 Localized Assets
- [ ] Localized images (if any text in images)
- [ ] Localized audio (optional)
- [ ] Cultural considerations
- [ ] Date/Time format localization
- [ ] Number format localization

---

### Module 29: Performance & Optimization

#### 29.1 Rendering Optimization
- [ ] Sprite batching
- [ ] Texture atlasing
- [ ] Object pooling
- [ ] LOD system (if 3D elements)
- [ ] Culling optimization
- [ ] Draw call optimization

#### 29.2 Memory Optimization
- [ ] Asset loading optimization
- [ ] Memory pooling
- [ ] Garbage collection optimization
- [ ] Texture compression
- [ ] Audio compression

#### 29.3 Network Optimization
- [ ] Message compression
- [ ] Delta updates
- [ ] Request batching
- [ ] Connection pooling
- [ ] Retry strategies

#### 29.4 Platform-specific Optimization
- [ ] Desktop optimization
- [ ] Mobile optimization
- [ ] WebGL optimization
- [ ] Low-end device support
- [ ] Quality presets

#### 29.5 Performance Monitoring
- [ ] FPS monitoring
- [ ] Memory usage monitoring
- [ ] Network latency monitoring
- [ ] Analytics integration
- [ ] Crash reporting

---

### Module 30: Quality Assurance & Testing

#### 30.1 Unit Testing
- [ ] Game logic unit tests
- [ ] Movement validation tests
- [ ] Capture logic tests
- [ ] Score calculation tests
- [ ] Win condition tests

#### 30.2 Integration Testing
- [ ] UI integration tests
- [ ] Network integration tests
- [ ] Save/Load tests
- [ ] Platform-specific tests

#### 30.3 Manual Testing
- [ ] Feature testing checklist
- [ ] Regression testing
- [ ] Usability testing
- [ ] Accessibility testing
- [ ] Device testing matrix

#### 30.4 Beta Testing
- [ ] Closed beta recruitment
- [ ] Beta feedback collection
- [ ] Bug reporting system
- [ ] Beta patch deployment
- [ ] Beta-to-release transition

#### 30.5 Testing Tools
- [ ] Automated test framework
- [ ] Test coverage reporting
- [ ] Bug tracking system
- [ ] Test case management
- [ ] CI/CD integration for tests

---

## Backend Infrastructure (C# Custom Server)
**Priority: High (parallel với Phase 2)**

### Module 31: Server Architecture

#### 31.1 RESTful API
- [ ] API framework setup (ASP.NET Core)
- [ ] API versioning
- [ ] Request/Response formatting (JSON)
- [ ] Error handling standardization
- [ ] API documentation (Swagger)
- [ ] Rate limiting
- [ ] Request validation

#### 31.2 API Endpoints - Authentication
- [ ] POST /api/auth/register
- [ ] POST /api/auth/login
- [ ] POST /api/auth/logout
- [ ] POST /api/auth/refresh-token
- [ ] POST /api/auth/forgot-password
- [ ] POST /api/auth/reset-password
- [ ] PUT /api/auth/change-password

#### 31.3 API Endpoints - Profile
- [ ] GET /api/profile/{id}
- [ ] PUT /api/profile/{id}
- [ ] GET /api/profile/{id}/stats
- [ ] GET /api/profile/{id}/match-history
- [ ] DELETE /api/profile/{id}

#### 31.4 API Endpoints - Social
- [ ] GET /api/friends
- [ ] POST /api/friends/request
- [ ] PUT /api/friends/request/{id}/accept
- [ ] DELETE /api/friends/{id}
- [ ] GET /api/leaderboard

#### 31.5 WebSocket Server
- [ ] WebSocket framework setup (SignalR)
- [ ] Connection management
- [ ] Authentication for WebSocket
- [ ] Message routing
- [ ] Room management
- [ ] Broadcast system
- [ ] Heartbeat/Keep-alive

#### 31.6 WebSocket Events - Game
- [ ] game:create
- [ ] game:join
- [ ] game:leave
- [ ] game:start
- [ ] game:move
- [ ] game:chat
- [ ] game:end
- [ ] game:reconnect

#### 31.7 WebSocket Events - Matchmaking
- [ ] matchmaking:join
- [ ] matchmaking:leave
- [ ] matchmaking:found
- [ ] matchmaking:accept
- [ ] matchmaking:decline

#### 31.8 Database Design
- [ ] Database selection (PostgreSQL/MySQL)
- [ ] ORM setup (Entity Framework Core)
- [ ] Users table
- [ ] Profiles table
- [ ] Matches table
- [ ] Replays table
- [ ] Friends table
- [ ] Leaderboard table (materialized view)
- [ ] Sessions table
- [ ] Database migrations
- [ ] Database indexes optimization

---

### Module 32: Server Features

#### 32.1 Authentication Service
- [ ] User registration logic
- [ ] Password hashing (bcrypt)
- [ ] Email verification service
- [ ] JWT token generation
- [ ] Token validation middleware
- [ ] Refresh token mechanism
- [ ] Session management

#### 32.2 ELO Rating Service
- [ ] ELO calculation algorithm
- [ ] K-factor configuration
- [ ] ELO update on match end
- [ ] ELO history tracking
- [ ] Rank calculation from ELO
- [ ] Provisional rating for new players

#### 32.3 Matchmaking Service
- [ ] Matchmaking queue management
- [ ] ELO-based matching algorithm
- [ ] Queue timeout handling
- [ ] Match creation
- [ ] Player notification
- [ ] Queue statistics

#### 32.4 Game Session Service
- [ ] Game state management
- [ ] Move validation
- [ ] Turn management
- [ ] Game result determination
- [ ] AFK detection
- [ ] Reconnection handling
- [ ] Game cleanup

#### 32.5 Replay Storage Service
- [ ] Replay data compression
- [ ] Replay storage (database or file)
- [ ] Replay retrieval
- [ ] Replay sharing
- [ ] Replay cleanup (old replays)
- [ ] Storage quota management

#### 32.6 Chat Service
- [ ] Message handling
- [ ] Profanity filtering
- [ ] Rate limiting
- [ ] Chat history (temporary)
- [ ] Report handling

---

### Module 33: DevOps & Hosting

#### 33.1 Cloud Hosting Setup
- [ ] Cloud provider selection (AWS/Azure/GCP)
- [ ] Server provisioning
- [ ] Load balancer setup (if needed)
- [ ] Database hosting
- [ ] CDN for static assets (optional)
- [ ] SSL certificate setup

#### 33.2 Containerization
- [ ] Docker configuration
- [ ] Docker Compose for local development
- [ ] Container registry setup
- [ ] Container orchestration (optional - Kubernetes)

#### 33.3 CI/CD Pipeline
- [ ] Source control (Git)
- [ ] Build automation
- [ ] Test automation
- [ ] Deployment automation
- [ ] Environment management (dev, staging, prod)
- [ ] Rollback strategy

#### 33.4 Monitoring & Logging
- [ ] Application logging
- [ ] Log aggregation
- [ ] Error tracking (Sentry)
- [ ] Performance monitoring
- [ ] Uptime monitoring
- [ ] Alert system

#### 33.5 Security
- [ ] Firewall configuration
- [ ] DDoS protection
- [ ] Security updates schedule
- [ ] Backup strategy
- [ ] Disaster recovery plan

---

### Module 34: Cost Optimization Strategies

#### 34.1 Free Tier Utilization
- [ ] Cloud provider free tier analysis
  - [ ] AWS Free Tier (EC2, RDS, etc.)
  - [ ] Azure Free Tier
  - [ ] GCP Free Tier
  - [ ] Other providers (Railway, Render, Fly.io)
- [ ] Database free tier options
  - [ ] Supabase (500MB free)
  - [ ] PlanetScale (5GB free)
  - [ ] MongoDB Atlas (512MB free)
- [ ] Maximum free tier utilization plan

#### 34.2 Resource Optimization
- [ ] Right-sizing instances
- [ ] Auto-scaling configuration
- [ ] Spot/Preemptible instances (for non-critical)
- [ ] Reserved instances (if committed)
- [ ] Scheduled scaling

#### 34.3 Architecture Optimization
- [ ] Serverless functions where appropriate
- [ ] Edge caching
- [ ] Database query optimization
- [ ] Connection pooling
- [ ] Lazy loading strategies

#### 34.4 Cost Monitoring
- [ ] Budget alerts setup
- [ ] Cost dashboard
- [ ] Resource usage tracking
- [ ] Monthly cost review
- [ ] Cost optimization recommendations

#### 34.5 Development Phase Cost Plan
- [ ] Development environment (minimal cost)
- [ ] Staging environment (low cost)
- [ ] Production environment (optimized)
- [ ] Cost per active user estimation
- [ ] Break-even analysis

---

## Appendix

### A. Technology Stack

| Component | Technology |
|-----------|------------|
| Game Engine | Unity (2D) |
| Primary Language | C# |
| Backend Framework | ASP.NET Core |
| Real-time Communication | SignalR |
| Database | PostgreSQL / MySQL |
| ORM | Entity Framework Core |
| Authentication | JWT |
| API Documentation | Swagger |
| Containerization | Docker |
| Version Control | Git |
| CI/CD | GitHub Actions / Azure DevOps |

### B. Priority Matrix

| Priority | Phase | Description |
|----------|-------|-------------|
| P0 - Critical | Phase 1 | MVP features, must have for initial release |
| P1 - High | Phase 2, Backend | Online features, required for multiplayer |
| P2 - Medium | Phase 3 | Enhancement features, improve player experience |
| P3 - Low | Phase 4 | Future features, expansion and polish |

### C. Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Complex game rules implementation | High | High | Thorough testing, iterative development |
| Network latency issues | Medium | High | Client-side prediction, server optimization |
| Cross-platform compatibility | Medium | Medium | Early testing on target platforms |
| Cost overrun (hosting) | Low | Medium | Free tier utilization, cost monitoring |
| Scalability issues | Low | High | Modular architecture, load testing |

### D. Definition of Done

Một feature được coi là hoàn thành khi:
1. ✅ Code được viết và reviewed
2. ✅ Unit tests passed
3. ✅ Integration tests passed (nếu applicable)
4. ✅ Manual testing completed
5. ✅ Documentation updated
6. ✅ Performance acceptable
7. ✅ No critical bugs
8. ✅ Merged to main branch

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2024-12-06 | Initial feature list creation |

---

*Document created for Commander Chess project. Total: 34 modules, ~1033 features.*
