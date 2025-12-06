# Commander Chess - Feature List

> **Tổng quan**: 34 Modules | 1000+ Features | 4 Phases phát triển

## Mục lục

- [Phase 1: MVP - Core Gameplay & DevMode](#phase-1-mvp---core-gameplay--devmode)
- [Phase 2: Online & Social Features](#phase-2-online--social-features)
- [Phase 3: Advanced Features & Content](#phase-3-advanced-features--content)
- [Phase 4: Cross-platform & Polish](#phase-4-cross-platform--polish)
- [Backend Infrastructure](#backend-infrastructure-c-custom-server)
- [Roadmap](#roadmap)
- [Technology Stack](#technology-stack)
- [Ghi chú](#ghi-chú)

---

## Chú thích Priority

| Icon | Priority | Mô tả |
|------|----------|-------|
| 🔴 | Critical | Bắt buộc phải có |
| 🟠 | High | Quan trọng, nên có |
| 🟡 | Medium | Có thể trì hoãn |
| 🟢 | Low/Future | Nice-to-have |

---

## Phase 1: MVP - Core Gameplay & DevMode

### Module 1: Game Board System
> Hệ thống bàn cờ 12x11 với đầy đủ các loại địa hình

#### 1.1 Board Layout 🔴
- [ ] 12x11 grid display (VA: 0-11, HA: 0-10)
- [ ] VA/HA coordinate system implementation
- [ ] Origin point (0,0) at bottom-left corner
- [ ] Coordinate labels on board edges
- [ ] Grid line rendering

#### 1.2 Terrain Types 🔴
- [ ] Land terrain (default)
- [ ] Sea terrain (blue area)
- [ ] River terrain (separating two sides)
- [ ] Deep water river segments
- [ ] Shallow/reef-base river segments (2 crossing points)
- [ ] Terrain visual differentiation
- [ ] Terrain data structure

#### 1.3 Board Display 🔴
- [ ] 2D top-down view
- [ ] Terrain texture/color mapping
- [ ] Grid overlay toggle
- [ ] Board border decoration
- [ ] Background layer

#### 1.4 Highlighting System 🟠
- [ ] Selected piece highlight
- [ ] Valid move positions highlight
- [ ] Valid capture positions highlight (different color)
- [ ] Last move highlight
- [ ] Check/checkmate highlight on Commander
- [ ] Ring of fire visualization
- [ ] Hover highlight

#### 1.5 Camera Controls 🟠
- [ ] Zoom in/out functionality
- [ ] Pan/drag to move view
- [ ] Fit-to-screen option
- [ ] Camera bounds restriction
- [ ] Smooth camera transitions
- [ ] Reset view button

#### 1.6 Board Orientation 🟡
- [ ] Default orientation (Blue at bottom)
- [ ] Flip board option
- [ ] Auto-flip for current player (optional)
- [ ] Rotation animation

---

### Module 2: Piece System
> 11 loại quân cờ với đầy đủ attributes và abilities

#### 2.1 Piece Data Structure 🔴
- [ ] Base Piece class/scriptable object
- [ ] Piece type enum (11 types)
- [ ] Owner/side property (Red/Blue)
- [ ] Current position property
- [ ] Movement range property
- [ ] Capture range property
- [ ] Score value property
- [ ] Special abilities flags

#### 2.2 Commander (Tư Lệnh) 🔴
- [ ] Movement: unlimited along VA/HA (no blockage)
- [ ] Capture: 1 segment only
- [ ] Cannot move diagonally
- [ ] Can enter Headquarter
- [ ] Cannot face opposing Commander directly
- [ ] Score: 100 points
- [ ] Visual design & sprite

#### 2.3 Infantry (Bộ Binh) 🔴
- [ ] Movement: 1 segment (VA/HA only)
- [ ] Capture: 1 segment
- [ ] Can cross deep water freely
- [ ] Can be carried by Tank/Air Force/Navy
- [ ] Score: 10 points
- [ ] Quantity: 2 per side
- [ ] Visual design & sprite

#### 2.4 Tank (Xe Tăng) 🔴
- [ ] Movement: 1-2 segments (VA/HA)
- [ ] Capture: 1-2 segments
- [ ] Can capture at sea without moving (ranged attack)
- [ ] Can cross deep water freely
- [ ] Can carry up to 2 troops (Infantry/Militia/Commander)
- [ ] Score: 20 points
- [ ] Quantity: 2 per side
- [ ] Visual design & sprite

#### 2.5 Militia (Dân Quân) 🔴
- [ ] Movement: 1 segment (all 8 directions)
- [ ] Capture: 1 segment (all 8 directions)
- [ ] Can cross deep water freely
- [ ] Can be carried by Tank/Air Force/Navy
- [ ] Score: 10 points
- [ ] Quantity: 1 per side
- [ ] Visual design & sprite

#### 2.6 Engineer (Công Binh) 🔴
- [ ] Movement: 1 segment (VA/HA only)
- [ ] Capture: 1 segment
- [ ] Can carry heavy vehicles across river
- [ ] Score: 10 points
- [ ] Quantity: 2 per side
- [ ] Visual design & sprite

#### 2.7 Artillery (Pháo Binh) 🔴
- [ ] Movement: 1-3 segments (all 8 directions)
- [ ] Capture: 1-3 segments
- [ ] Must use reef-base to cross river (unless capturing)
- [ ] Can capture across river when attacking
- [ ] Can attack sea without moving (ranged attack)
- [ ] Score: 30 points
- [ ] Quantity: 2 per side
- [ ] Visual design & sprite

#### 2.8 Anti-aircraft (Phòng Không) 🔴
- [ ] Movement: 1 segment (VA/HA only)
- [ ] Capture: 1 segment
- [ ] Ring of fire: 1 segment radius
- [ ] Cannot cross deep water alone
- [ ] Can be carried by Engineer
- [ ] Score: 10 points
- [ ] Quantity: 2 per side
- [ ] Visual design & sprite

#### 2.9 Missile (Tên Lửa) 🔴
- [ ] Movement: within 2-segment ring
- [ ] Capture: within 2-segment ring (ground & sky)
- [ ] Ring of fire: 2 segments (1 segment diagonal)
- [ ] Cannot cross deep water alone
- [ ] Can be carried by Engineer
- [ ] Score: 20 points
- [ ] Quantity: 1 per side
- [ ] Visual design & sprite

#### 2.10 Air Force (Không Quân) 🔴
- [ ] Movement: 1-4 segments (all 8 directions)
- [ ] Capture: 1-4 segments
- [ ] Can fly over any blockage
- [ ] Destroyed by ring of fire (unless stealth)
- [ ] Can return to original position if unsafe landing on land piece
- [ ] Cannot land on sea
- [ ] Can be carried by Navy (safe from ring of fire)
- [ ] Can carry up to 2 troops
- [ ] Score: 40 points
- [ ] Quantity: 2 per side
- [ ] Visual design & sprite

#### 2.11 Navy (Hải Quân) 🔴
- [ ] Movement: 1-4 segments (all 8 directions)
- [ ] Capture: multiple weapons
- [ ] Anti-aircraft: 1 segment ring of fire
- [ ] Gunboat: 1-3 segments (like Artillery)
- [ ] Anti-ship missile: 1-4 segments (Navy only)
- [ ] Can enter deep river (not reef-base)
- [ ] Not blocked by friendly pieces when moving
- [ ] Can carry up to 2 troops (including Air Force)
- [ ] Score: 80 points (10+30+40)
- [ ] Quantity: 2 per side
- [ ] Visual design & sprite

#### 2.12 Headquarter (Sở Chỉ Huy) 🔴
- [ ] Cannot move
- [ ] Cannot capture
- [ ] Only Commander can enter
- [ ] Provides protection for Commander
- [ ] Can become heroic (last defender)
- [ ] Score: 0 points (non-capturable normally)
- [ ] Quantity: 2 per side
- [ ] Visual design & sprite

#### 2.13 Piece Rendering 🔴
- [ ] Sprite loading system
- [ ] Red/Blue color differentiation
- [ ] Piece scaling based on zoom
- [ ] Piece sorting order (z-index)
- [ ] Combined piece visual (stacked pieces)
- [ ] Heroic piece visual indicator

#### 2.14 Piece Animation States 🟠
- [ ] Idle animation
- [ ] Selected animation (bounce/glow)
- [ ] Heroic status animation
- [ ] Carried status indicator

---

### Module 3: Core Game Rules Implementation
> Logic xử lý luật chơi Commander Chess

#### 3.1 Movement System 🔴
- [ ] Movement validation based on piece type
- [ ] Path obstruction checking
- [ ] Diagonal movement support (where applicable)
- [ ] Movement range calculation
- [ ] Valid moves generation for selected piece

#### 3.2 Capture System 🔴
- [ ] Capture validation based on piece type
- [ ] Ranged capture (Tank, Artillery to sea)
- [ ] Capture with position replacement
- [ ] Capture without moving (standing still attack)
- [ ] Valid captures generation for selected piece
- [ ] Score calculation on capture

#### 3.3 River Crossing Rules 🔴
- [ ] Reef-base crossing (all pieces)
- [ ] Deep water crossing (Infantry, Militia, Tank, Commander)
- [ ] Heavy vehicle restriction (Artillery, Anti-aircraft, Missile)
- [ ] Engineer carry system for heavy vehicles
- [ ] Cross-river capture exception

#### 3.4 Ring of Fire System 🔴
- [ ] Anti-aircraft ring (1 segment radius)
- [ ] Missile ring (2 segments, 1 diagonal)
- [ ] Navy anti-aircraft ring
- [ ] Air Force destruction on ring entry
- [ ] Mutual destruction (Air Force vs ring piece)
- [ ] Ring visualization
- [ ] Safe zones (carried by Navy)

#### 3.5 Terrific Speed Operation (Combined Pieces) 🔴
- [ ] Piece combination (Tank + Infantry/Militia/Commander)
- [ ] Air Force combination
- [ ] Navy carrier system
- [ ] Skip turn to combine
- [ ] Maximum 2 carried pieces
- [ ] Split from same point
- [ ] Multiple captures in one turn (up to 3)
- [ ] Combined piece score calculation

#### 3.6 Heroic Piece System 🔴
- [ ] Check detection for heroic promotion
- [ ] Last defender heroic promotion
- [ ] Heroic Infantry: +1 movement, all directions
- [ ] Heroic Tank: +1 movement (total 3)
- [ ] Heroic Air Force: stealth (ignore ring of fire)
- [ ] Heroic Headquarter: can move & capture (2 segments)
- [ ] Heroic visual indicator
- [ ] Heroic status persistence

#### 3.7 Commander Special Rules 🔴
- [ ] Commander cannot face opponent Commander
- [ ] Commander line-of-sight checking
- [ ] Commander in Headquarter logic
- [ ] Commander can ignore sea when joining Navy
- [ ] Check detection
- [ ] Checkmate detection

#### 3.8 Turn System 🔴
- [ ] Turn order management (Red first by default)
- [ ] Turn state machine
- [ ] Move execution
- [ ] Turn completion validation
- [ ] Turn history recording

#### 3.9 Win Condition Detection 🔴
- [ ] Commander capture = victory
- [ ] Score-based victory (time expired)
- [ ] Tactical mode objectives
- [ ] Draw conditions (if any)

---

### Module 4: Game Mode - Standard
> Chế độ chơi chuẩn Total Force 30 phút

#### 4.1 Total Force Mode (30 minutes) 🔴
- [ ] Full piece setup (38 pieces total)
- [ ] 30-minute total time limit
- [ ] Commander capture = immediate win (+50 points)
- [ ] Time expiry = score comparison
- [ ] Victory bonus: +50 points

#### 4.2 Timer System 🔴
- [ ] Total time countdown (30 min each)
- [ ] Turn timer optional
- [ ] Timer pause on game pause
- [ ] Timer display (MM:SS format)
- [ ] Low time warning (visual + audio)
- [ ] Time expiry handling

#### 4.3 Scoring System 🔴
- [ ] Real-time score tracking
- [ ] Score display for both players
- [ ] Score update on capture
- [ ] Combined piece score calculation
- [ ] Victory bonus calculation
- [ ] Final score display

#### 4.4 Win Conditions 🔴
- [ ] Primary: Commander captured
- [ ] Secondary: Higher score at time limit
- [ ] Tie-breaker rules
- [ ] Victory message display
- [ ] Statistics recording

#### 4.5 Standard Piece Setup 🔴
- [ ] Initial positions for all 38 pieces
- [ ] Symmetric setup (mirrored)
- [ ] Setup validation
- [ ] Setup visualization

---

### Module 5: Local Multiplayer
> Chế độ Hot-seat 2 người chơi cùng thiết bị

#### 5.1 Hot-seat Mode 🔴
- [ ] Two players same device
- [ ] Turn-based control switching
- [ ] Current player indicator
- [ ] Board auto-flip option

#### 5.2 Turn Confirmation 🔴
- [ ] Move preview before confirm
- [ ] Confirm button
- [ ] Cancel/undo before confirm
- [ ] End turn button

#### 5.3 Player Information Display 🔴
- [ ] Player names (customizable)
- [ ] Player colors (Red/Blue)
- [ ] Current turn indicator
- [ ] Score display per player
- [ ] Time display per player
- [ ] Captured pieces display

#### 5.4 Pass Device Prompt 🟠
- [ ] "Pass to opponent" screen
- [ ] Hide board during pass (optional)
- [ ] Ready confirmation from next player

---

### Module 6: UI/UX Core
> Giao diện người dùng cơ bản

#### 6.1 Main Menu 🔴
- [ ] Play button (leads to mode selection)
- [ ] Tutorial button
- [ ] Settings button
- [ ] Exit/Quit button
- [ ] Version display
- [ ] Credits button

#### 6.2 Mode Selection Screen 🔴
- [ ] Local Multiplayer option
- [ ] Online Multiplayer option (greyed Phase 1)
- [ ] Tutorial/Practice option
- [ ] Mode descriptions
- [ ] Back button

#### 6.3 Game HUD 🔴
- [ ] Player 1 info panel (name, score, time, captured)
- [ ] Player 2 info panel
- [ ] Current turn indicator
- [ ] Move history panel
- [ ] Pause button
- [ ] Settings quick access

#### 6.4 Move History Panel 🟠
- [ ] List of moves in notation
- [ ] Scroll functionality
- [ ] Click to highlight move on board
- [ ] Export option (future)

#### 6.5 Pause Menu 🔴
- [ ] Resume button
- [ ] Restart game button
- [ ] Settings button
- [ ] Forfeit/Resign button
- [ ] Return to main menu button
- [ ] Game state preservation

#### 6.6 Victory/Defeat Screen 🔴
- [ ] Winner announcement
- [ ] Final scores display
- [ ] Victory condition (Commander captured / Score)
- [ ] Play again button
- [ ] Return to menu button
- [ ] Share result (future)

#### 6.7 Settings Menu 🔴
- [ ] Audio settings (volume sliders)
- [ ] Display settings
- [ ] Game settings
- [ ] DevMode toggle (hidden/password)
- [ ] Apply/Cancel buttons
- [ ] Reset to defaults

#### 6.8 Confirmation Dialogs 🔴
- [ ] Exit game confirmation
- [ ] Forfeit confirmation
- [ ] Restart confirmation
- [ ] Standard Yes/No/Cancel buttons

#### 6.9 Loading Screen 🟡
- [ ] Loading indicator
- [ ] Loading progress (if applicable)
- [ ] Tips display (optional)

---

### Module 7: Visual Effects 2D
> Hiệu ứng hình ảnh 2D

#### 7.1 Movement Animations 🔴
- [ ] Piece slide animation
- [ ] Speed based on distance
- [ ] Flying animation for Air Force
- [ ] Naval movement (water ripple)
- [ ] Combined piece movement

#### 7.2 Capture Animations 🔴
- [ ] Capture impact effect
- [ ] Captured piece fade/destroy
- [ ] Explosion effect (vehicles)
- [ ] Air Force crash effect
- [ ] Naval sink effect

#### 7.3 Ring of Fire Effects 🔴
- [ ] Ring visualization (circle/highlight)
- [ ] Air Force entering ring effect
- [ ] Mutual destruction effect
- [ ] Ring pulse animation

#### 7.4 Heroic Piece Effects 🔴
- [ ] Promotion animation (glow/particles)
- [ ] Heroic idle effect
- [ ] Stealth Air Force effect

#### 7.5 UI Animations 🟠
- [ ] Button hover/press effects
- [ ] Panel slide animations
- [ ] Score update animation
- [ ] Timer warning animation
- [ ] Victory celebration effect

#### 7.6 Board Effects 🟡
- [ ] Water animation (waves)
- [ ] Terrain ambient effects
- [ ] Check warning effect
- [ ] Checkmate effect

---

### Module 8: Audio System
> Hệ thống âm thanh

#### 8.1 Background Music 🔴
- [ ] Menu theme
- [ ] Gameplay theme
- [ ] Victory theme
- [ ] Defeat theme
- [ ] Tension theme (low time/check)
- [ ] Music loop handling
- [ ] Music crossfade

#### 8.2 Sound Effects - UI 🔴
- [ ] Button click
- [ ] Button hover
- [ ] Menu open/close
- [ ] Notification sound
- [ ] Error/invalid action sound
- [ ] Victory fanfare
- [ ] Defeat sound

#### 8.3 Sound Effects - Gameplay 🔴
- [ ] Piece select
- [ ] Piece move (different per type)
- [ ] Piece capture
- [ ] Air Force flying
- [ ] Naval movement (water)
- [ ] Tank movement (engine)
- [ ] Artillery fire
- [ ] Missile launch
- [ ] Explosion
- [ ] Ring of fire activation
- [ ] Check warning
- [ ] Checkmate sound
- [ ] Turn change

#### 8.4 Sound Effects - Heroic 🟠
- [ ] Heroic promotion sound
- [ ] Heroic piece special sounds

#### 8.5 Volume Controls 🔴
- [ ] Master volume
- [ ] Music volume
- [ ] SFX volume
- [ ] Mute all option
- [ ] Volume persistence (save settings)

#### 8.6 Audio Manager 🔴
- [ ] Audio source pooling
- [ ] 3D audio (optional)
- [ ] Audio priority system
- [ ] Audio ducking

---

### Module 9: Tutorial System
> Hệ thống hướng dẫn tương tác

#### 9.1 Interactive Tutorials 🔴
- [ ] Welcome/Introduction
- [ ] Board & coordinates tutorial
- [ ] Basic piece movement tutorial
- [ ] Capture mechanics tutorial
- [ ] River crossing tutorial
- [ ] Ring of fire tutorial
- [ ] Combined pieces tutorial
- [ ] Heroic pieces tutorial
- [ ] Commander rules tutorial
- [ ] Win conditions tutorial

#### 9.2 Tutorial Flow 🔴
- [ ] Step-by-step guidance
- [ ] Highlight target areas
- [ ] Restrict invalid actions during tutorial
- [ ] Progress tracking
- [ ] Skip option
- [ ] Replay individual tutorials

#### 9.3 Practice Mode 🟠
- [ ] Scenario: Basic movement
- [ ] Scenario: Capture practice
- [ ] Scenario: River crossing
- [ ] Scenario: Ring of fire
- [ ] Scenario: Combined attacks
- [ ] Scenario: Checkmate practice
- [ ] Scenario: Heroic activation
- [ ] Free practice (no opponent)
- [ ] Practice vs simple AI (future)

#### 9.4 Hint System 🟡
- [ ] Show valid moves hint
- [ ] Suggest good moves
- [ ] Explain why move is invalid
- [ ] Toggle hints on/off

#### 9.5 Tutorial Completion 🟠
- [ ] Tutorial progress save
- [ ] Completion badges/indicators
- [ ] Recommend next tutorial
- [ ] Tutorial menu

---

### Module 10: DevMode - Game Designer Tools ⭐
> Công cụ phát triển và thiết kế game

#### 10.1 DevMode Access & Security 🔴
- [ ] Password protection to enable DevMode
- [ ] DevMode toggle in settings (hidden by default)
- [ ] Visual indicator when DevMode is active
- [ ] DevMode session logging
- [ ] DevMode access levels (optional)

#### 10.2 Piece Customization 🔴
- [ ] Modify movement range (any piece type)
- [ ] Modify capture range (any piece type)
- [ ] Modify score value (any piece type)
- [ ] Toggle ring of fire ability
- [ ] Toggle flying ability
- [ ] Toggle swimming ability
- [ ] Toggle carry ability
- [ ] Toggle can-be-carried ability
- [ ] Custom piece attributes editor
- [ ] Save piece definition as JSON
- [ ] Load piece definition from JSON
- [ ] Export piece set as package
- [ ] Import piece set package
- [ ] Reset piece to default
- [ ] Piece preset library

#### 10.3 Board Customization 🔴
- [ ] Board size adjustment (width: 8-20)
- [ ] Board size adjustment (height: 8-20)
- [ ] Terrain painting - brush tool
- [ ] Terrain painting - fill tool
- [ ] Terrain painting - rectangle tool
- [ ] Terrain painting - eraser
- [ ] Terrain types: land, sea, river-deep, river-shallow
- [ ] Custom terrain type creation
- [ ] Undo/redo for board editing
- [ ] Save board layout as JSON
- [ ] Load board layout from JSON
- [ ] Export board as image
- [ ] Import board from image (optional)
- [ ] Board preset library
- [ ] Validate board (connectivity check)

#### 10.4 Game Rules Customization 🔴
- [ ] Win condition editor
  - [ ] Commander capture toggle
  - [ ] Score threshold victory
  - [ ] Piece count victory
  - [ ] Custom victory conditions
- [ ] Time control settings
  - [ ] Total time per player
  - [ ] Time increment per turn
  - [ ] Turn time limit
  - [ ] No timer mode
- [ ] Score system customization
  - [ ] Piece score values
  - [ ] Victory bonus
  - [ ] Capture multipliers
- [ ] Feature toggles
  - [ ] Enable/disable heroic system
  - [ ] Enable/disable combined pieces
  - [ ] Enable/disable ring of fire
  - [ ] Enable/disable river crossing rules
  - [ ] Enable/disable Commander facing rule
- [ ] Save rules preset as JSON
- [ ] Load rules preset from JSON
- [ ] Rules preset library

#### 10.5 Debug Tools 🔴
- [ ] **Game State Inspector**
  - [ ] View all piece positions
  - [ ] View piece attributes live
  - [ ] View game phase/turn
  - [ ] View score breakdown
  - [ ] View timer states
  - [ ] View move history detailed
- [ ] **Piece Manipulation**
  - [ ] Spawn any piece at position
  - [ ] Remove piece from board
  - [ ] Move piece to any position
  - [ ] Change piece owner (Red/Blue)
  - [ ] Toggle piece heroic status
  - [ ] Create combined pieces
  - [ ] Split combined pieces
- [ ] **Game Flow Control**
  - [ ] Force end turn
  - [ ] Skip to next turn
  - [ ] Undo last move
  - [ ] Redo move
  - [ ] Jump to specific turn number
  - [ ] Reset game to initial state
- [ ] **Timer/Score Manipulation**
  - [ ] Set player time
  - [ ] Add/remove time
  - [ ] Pause/resume timer
  - [ ] Set player score
  - [ ] Add/remove score
- [ ] **Visual Debug Overlays**
  - [ ] Show all valid moves for any piece
  - [ ] Show all rings of fire
  - [ ] Show piece threat ranges
  - [ ] Show coordinate grid
  - [ ] Show piece IDs
  - [ ] Show terrain type overlay
- [ ] **Console Log Window**
  - [ ] Real-time game event log
  - [ ] Filter by event type
  - [ ] Search logs
  - [ ] Export logs
  - [ ] Clear logs
- [ ] **Network Simulation** (for future online)
  - [ ] Simulate latency
  - [ ] Simulate packet loss
  - [ ] Simulate disconnection
- [ ] **Save/Load States**
  - [ ] Save current game state
  - [ ] Load saved game state
  - [ ] Quick save slot (F5)
  - [ ] Quick load slot (F9)
  - [ ] Multiple save slots
  - [ ] Export state as file
  - [ ] Import state from file

#### 10.6 Custom Game Mode Creator 🟠
- [ ] Create new mode from template
- [ ] Assign board layout preset
- [ ] Assign piece configuration preset
- [ ] Assign rules preset
- [ ] Set mode name and description
- [ ] Set mode icon/thumbnail
- [ ] Test mode in sandbox
- [ ] Validate mode settings
- [ ] Export mode as package
- [ ] Import mode package
- [ ] Share mode (future online)

#### 10.7 DevMode Quality of Life 🟠
- [ ] **Hotkeys**
  - [ ] F1: Toggle debug panel
  - [ ] F5: Quick save
  - [ ] F9: Quick load
  - [ ] Ctrl+R: Reset game
  - [ ] Ctrl+Z: Undo
  - [ ] Ctrl+Y: Redo
  - [ ] Ctrl+D: Toggle DevMode UI
  - [ ] Customizable hotkeys
- [ ] **Preset Management**
  - [ ] Favorite presets
  - [ ] Recent presets history
  - [ ] Preset categories/tags
  - [ ] Search presets
- [ ] **Built-in Documentation**
  - [ ] Tooltips for all DevMode features
  - [ ] Help panel with usage guide
  - [ ] Keyboard shortcuts reference
  - [ ] Video tutorials links (optional)

---

## Phase 2: Online & Social Features

### Module 11: Authentication System
> Hệ thống xác thực người dùng

#### 11.1 Email/Password Registration 🔴
- [ ] Registration form (email, password, confirm password)
- [ ] Email validation
- [ ] Password strength requirements
- [ ] Terms of service agreement
- [ ] Registration API integration
- [ ] Success/error handling
- [ ] Email verification flow

#### 11.2 Login System 🔴
- [ ] Login form (email, password)
- [ ] Remember me option
- [ ] Login API integration
- [ ] JWT token storage
- [ ] Token refresh mechanism
- [ ] Auto-login on app start
- [ ] Login error handling

#### 11.3 Password Recovery 🔴
- [ ] Forgot password link
- [ ] Password reset request (email)
- [ ] Reset code/link validation
- [ ] New password form
- [ ] Success confirmation

#### 11.4 Session Management 🔴
- [ ] Session timeout handling
- [ ] Multi-device session (optional)
- [ ] Logout functionality
- [ ] Force logout (security)

#### 11.5 OAuth Integration 🟡
- [ ] Google sign-in
- [ ] Facebook sign-in
- [ ] Apple sign-in
- [ ] Account linking

---

### Module 12: Player Profile System
> Hệ thống hồ sơ người chơi

#### 12.1 Display Name 🔴
- [ ] Set display name on registration
- [ ] Change display name
- [ ] Name uniqueness validation
- [ ] Inappropriate name filter

#### 12.2 Avatar System 🟠
- [ ] Default avatars selection
- [ ] Custom avatar upload
- [ ] Avatar cropping tool
- [ ] Avatar size limits
- [ ] Avatar moderation (optional)

#### 12.3 Player Statistics 🔴
- [ ] Total games played
- [ ] Wins / Losses / Draws
- [ ] Win rate percentage
- [ ] Average game duration
- [ ] Favorite piece (most captures with)
- [ ] Longest win streak
- [ ] Current win streak
- [ ] Total playtime

#### 12.4 ELO Rating System 🔴
- [ ] Initial ELO (e.g., 1200)
- [ ] ELO calculation on match end
- [ ] ELO history graph
- [ ] Rank tiers based on ELO
- [ ] Rank icons/badges
- [ ] Provisional rating period

#### 12.5 Match History 🔴
- [ ] List of recent matches
- [ ] Match details (opponent, result, score, duration)
- [ ] Filter by result (win/loss)
- [ ] Filter by time period
- [ ] View match replay (link to replay system)
- [ ] Pagination

#### 12.6 Profile Privacy 🟡
- [ ] Public/private profile toggle
- [ ] Hide statistics option
- [ ] Block list management

---

### Module 13: Online Multiplayer - Room System
> Hệ thống phòng chơi online

#### 13.1 Create Room 🔴
- [ ] Room name input
- [ ] Room password (optional)
- [ ] Game mode selection
- [ ] Time control selection
- [ ] Side selection (Red/Blue/Random)
- [ ] Max spectators setting
- [ ] Create room API call
- [ ] Room code generation

#### 13.2 Join Room 🔴
- [ ] Room code input
- [ ] Password input (if required)
- [ ] Join room API call
- [ ] Room full handling
- [ ] Invalid room handling

#### 13.3 Room Browser 🔴
- [ ] List available rooms
- [ ] Filter by game mode
- [ ] Filter by ELO range
- [ ] Sort by (newest, players, ELO)
- [ ] Search by room name
- [ ] Refresh room list
- [ ] Auto-refresh option
- [ ] Pagination

#### 13.4 Room Lobby 🔴
- [ ] Display room info
- [ ] Player list (host, guest)
- [ ] Ready status toggle
- [ ] Side swap request
- [ ] Chat panel
- [ ] Kick player (host only)
- [ ] Leave room button
- [ ] Start game (when both ready)
- [ ] Countdown to start

#### 13.5 Room State Management 🔴
- [ ] Real-time room updates (WebSocket)
- [ ] Player join/leave notifications
- [ ] Ready state sync
- [ ] Room close handling
- [ ] Host migration (if host leaves)

---

### Module 14: Online Multiplayer - Matchmaking
> Hệ thống ghép trận tự động

#### 14.1 Quick Match 🔴
- [ ] One-click matchmaking
- [ ] Default game mode selection
- [ ] Searching indicator
- [ ] Cancel search option
- [ ] Match found notification
- [ ] Transition to game

#### 14.2 ELO-based Matchmaking 🔴
- [ ] ELO range matching
- [ ] Expanding search range over time
- [ ] Estimated wait time display
- [ ] Priority queue system

#### 14.3 Queue System 🔴
- [ ] Add to queue API
- [ ] Queue position display
- [ ] Queue timeout handling
- [ ] Multiple queue support (optional)

#### 14.4 Match Filters 🟠
- [ ] Game mode filter
- [ ] Time control filter
- [ ] ELO range preference
- [ ] Region preference (optional)
- [ ] Exclude recently played opponents

#### 14.5 Matchmaking Quality 🟡
- [ ] Match quality metrics
- [ ] Avoid repeated matchups
- [ ] Connection quality consideration
- [ ] Report matchmaking issues

---

### Module 15: Online Gameplay
> Gameplay online real-time

#### 15.1 Real-time Synchronization 🔴
- [ ] WebSocket connection
- [ ] Move synchronization
- [ ] Game state validation (server-side)
- [ ] Latency compensation
- [ ] State reconciliation
- [ ] Conflict resolution

#### 15.2 Reconnection System 🔴
- [ ] Connection loss detection
- [ ] Auto-reconnect attempts
- [ ] Reconnection grace period
- [ ] Resume game state
- [ ] Notify opponent of reconnection
- [ ] Manual reconnect option

#### 15.3 AFK Detection 🔴
- [ ] Idle time tracking
- [ ] AFK warning to player
- [ ] AFK timeout (auto-forfeit or pause)
- [ ] Notify opponent of AFK

#### 15.4 Forfeit/Resign 🔴
- [ ] Resign button
- [ ] Resignation confirmation
- [ ] Resign penalty (ELO loss)
- [ ] Notify opponent

#### 15.5 Draw Offer 🟠
- [ ] Offer draw button
- [ ] Draw offer notification
- [ ] Accept/decline draw
- [ ] Draw conditions

#### 15.6 Game End Handling 🔴
- [ ] Victory/defeat notification
- [ ] ELO change display
- [ ] Post-game options (rematch, return to lobby)
- [ ] Rate opponent (optional)

---

### Module 16: Chat System
> Hệ thống chat

#### 16.1 In-game Chat 🟠
- [ ] Chat input field
- [ ] Send message button
- [ ] Chat message display
- [ ] Timestamps
- [ ] Scroll functionality
- [ ] Real-time message sync

#### 16.2 Lobby Chat 🟠
- [ ] Room lobby chat
- [ ] Global lobby chat (optional)
- [ ] Chat history persistence

#### 16.3 Quick Emotes 🟠
- [ ] Predefined emote buttons
- [ ] "Good luck" emote
- [ ] "Well played" emote
- [ ] "Oops" emote
- [ ] "Thanks" emote
- [ ] "Thinking" emote
- [ ] Emote cooldown

#### 16.4 Profanity Filter 🔴
- [ ] Bad word detection
- [ ] Word replacement/censoring
- [ ] Multiple language support
- [ ] Filter sensitivity levels

#### 16.5 Mute Option 🔴
- [ ] Mute specific player
- [ ] Mute all chat
- [ ] Block player (persistent mute)
- [ ] Report player option

---

### Module 17: Spectator System
> Hệ thống xem trận đấu

#### 17.1 Real-time Spectating 🔴
- [ ] Join as spectator option
- [ ] Real-time move viewing
- [ ] Spectator-only UI
- [ ] Cannot interact with game

#### 17.2 Spectator Count 🟠
- [ ] Display number of spectators
- [ ] Spectator list
- [ ] Spectator join/leave notifications

#### 17.3 Spectator Controls 🟠
- [ ] View from either player perspective
- [ ] Analysis mode (show valid moves)
- [ ] Move prediction (hidden from game)

#### 17.4 View Match History 🟠
- [ ] Browse completed matches
- [ ] Watch replay of any public match
- [ ] Filter by player, date, etc.

#### 17.5 Stream Integration 🟢
- [ ] Spectator delay option
- [ ] Streamer mode (hide personal info)
- [ ] Featured matches

---

### Module 18: Leaderboard System
> Bảng xếp hạng

#### 18.1 Global Leaderboard 🔴
- [ ] Top players by ELO
- [ ] Player rank display
- [ ] Score/ELO display
- [ ] Profile link

#### 18.2 Leaderboard Filters 🟠
- [ ] Filter by time period (daily, weekly, monthly, all-time)
- [ ] Filter by game mode
- [ ] Filter by region (optional)
- [ ] Filter by rank tier

#### 18.3 Player Search 🟠
- [ ] Search by name
- [ ] View player profile from search
- [ ] Add friend from search

#### 18.4 Your Ranking 🔴
- [ ] Show your current rank
- [ ] Points to next rank
- [ ] Rank history
- [ ] Rank change notifications

#### 18.5 Seasonal Leaderboards 🟡
- [ ] Season system
- [ ] Season rewards
- [ ] Season reset

---

### Module 19: Friend System
> Hệ thống bạn bè

#### 19.1 Friend List 🔴
- [ ] Display friends list
- [ ] Online/offline status
- [ ] Last seen time
- [ ] Current activity (in game, in lobby)

#### 19.2 Add/Remove Friends 🔴
- [ ] Send friend request
- [ ] Accept/decline request
- [ ] Remove friend
- [ ] Friend request notifications
- [ ] Pending requests list

#### 19.3 Online Status 🔴
- [ ] Show online status
- [ ] Set status (online, away, invisible)
- [ ] Status sync real-time

#### 19.4 Invite to Game 🔴
- [ ] Invite friend to room
- [ ] Game invite notification
- [ ] Accept/decline invite
- [ ] Join friend's game

#### 19.5 Challenge Friend 🟠
- [ ] Send challenge
- [ ] Challenge settings (mode, time)
- [ ] Challenge notifications
- [ ] Accept/decline challenge
- [ ] Auto-create room on accept

#### 19.6 Friend Activity 🟡
- [ ] Activity feed
- [ ] Friend's recent matches
- [ ] Spectate friend's game

---

## Phase 3: Advanced Features & Content

### Module 20: Replay System
> Hệ thống xem lại trận đấu

#### 20.1 Auto-save Games 🔴
- [ ] Automatic game recording
- [ ] Save move-by-move data
- [ ] Save game metadata (players, result, duration)
- [ ] Local replay storage
- [ ] Cloud replay storage (online)

#### 20.2 Replay Player 🔴
- [ ] Play button
- [ ] Pause button
- [ ] Stop button
- [ ] Speed control (0.5x, 1x, 2x, 4x)
- [ ] Seek bar (timeline)
- [ ] Jump to move number
- [ ] Step forward (next move)
- [ ] Step backward (previous move)
- [ ] Jump to start
- [ ] Jump to end

#### 20.3 Replay Controls 🔴
- [ ] Current move indicator
- [ ] Total moves display
- [ ] Move notation display
- [ ] Board state at any point
- [ ] Show captured pieces at point
- [ ] Show score at point

#### 20.4 Share Replays 🟠
- [ ] Generate share link
- [ ] Share via social media
- [ ] Export replay file
- [ ] Import replay file
- [ ] Public/private replay setting

#### 20.5 Replay Analysis 🟡
- [ ] Show valid moves at each point
- [ ] Highlight key moments
- [ ] Add comments/annotations
- [ ] Save annotated replay

---

### Module 21: Custom Setup System
> Hệ thống thiết lập bàn cờ tùy chỉnh

#### 21.1 Drag-drop Piece Placement 🔴
- [ ] Piece palette (all piece types)
- [ ] Drag piece to board
- [ ] Remove piece from board
- [ ] Move placed pieces
- [ ] Red/Blue piece toggle
- [ ] Clear all button
- [ ] Clear side button

#### 21.2 Setup Validation 🔴
- [ ] Minimum piece requirements
- [ ] Commander required
- [ ] Piece count limits
- [ ] Valid position check (terrain)
- [ ] Validation error messages

#### 21.3 Save/Load Presets 🔴
- [ ] Save current setup
- [ ] Name setup preset
- [ ] Load saved preset
- [ ] Delete preset
- [ ] Preset list view

#### 21.4 Symmetric Setup 🟠
- [ ] Mirror placement mode
- [ ] Auto-mirror to opponent side
- [ ] Symmetry validation

#### 21.5 Random Generator 🟠
- [ ] Randomize all pieces
- [ ] Randomize one side
- [ ] Randomize with rules (e.g., keep Commander back)
- [ ] Seed-based random for replay

#### 21.6 Setup Sharing 🟡
- [ ] Export setup as code/file
- [ ] Import setup from code/file
- [ ] Share setup online

---

### Module 22: Additional Game Modes
> Các chế độ chơi bổ sung

#### 22.1 Tactical Mode (15 minutes) 🔴
- [ ] Full pieces, 15-minute time limit
- [ ] **Marine battle**: First to destroy 2 enemy Navy wins
- [ ] **Air battle**: First to destroy 2 enemy Air Force wins
- [ ] **Land battle**: First to destroy 2 Tank + 2 Infantry + 2 Artillery wins
- [ ] **Raid battle**: First to capture Commander wins
- [ ] Tactical victory: +50 bonus points
- [ ] Score comparison if no tactical win

#### 22.2 Sea/Air Battle Mode (9 pieces, 15 minutes) 🔴
- [ ] Limited piece setup (9 pieces per side)
- [ ] Sea battle: 2 Navy focus
- [ ] Air battle: 2 Air Force focus
- [ ] First to destroy 2 enemy Navy/Air Force wins
- [ ] 15-minute time limit
- [ ] Score-based tiebreaker

#### 22.3 Custom Game Modes 🟠
- [ ] Create mode from DevMode tools
- [ ] Play custom mode locally
- [ ] Share custom mode online
- [ ] Download community modes

#### 22.4 Daily/Weekly Challenges 🟡
- [ ] Pre-set puzzle scenarios
- [ ] Leaderboard for challenges
- [ ] Rewards for completion
- [ ] New challenges rotation

---

### Module 23: Achievements & Progression (Optional)
> Hệ thống thành tựu và tiến trình

#### 23.1 Achievement System 🟢
- [ ] Achievement categories
  - [ ] Gameplay achievements (wins, captures, etc.)
  - [ ] Collection achievements
  - [ ] Social achievements (friends, matches)
  - [ ] Challenge achievements
- [ ] Achievement tiers (bronze, silver, gold)
- [ ] Hidden achievements
- [ ] Achievement points

#### 23.2 Achievement Notifications 🟢
- [ ] Pop-up on achievement unlock
- [ ] Achievement sound
- [ ] Achievement gallery
- [ ] Progress tracking

#### 23.3 Rewards 🟢
- [ ] Cosmetic rewards (skins, themes)
- [ ] Title rewards
- [ ] Profile decoration rewards
- [ ] Achievement points rewards

#### 23.4 Player Level 🟢
- [ ] XP system
- [ ] Level progression
- [ ] Level rewards
- [ ] Level display on profile

---

### Module 24: Cosmetic Customization (Optional)
> Tùy chỉnh giao diện

#### 24.1 Piece Skins 🟢
- [ ] Alternative piece designs
- [ ] Skin shop/gallery
- [ ] Equip skins
- [ ] Preview skins

#### 24.2 Board Themes 🟢
- [ ] Alternative board designs
- [ ] Theme shop/gallery
- [ ] Equip themes
- [ ] Preview themes

#### 24.3 UI Themes 🟢
- [ ] Dark mode
- [ ] Light mode
- [ ] Custom color schemes
- [ ] Theme preview

#### 24.4 Sound Packs 🟢
- [ ] Alternative sound effects
- [ ] Alternative music
- [ ] Sound pack shop
- [ ] Preview sounds

#### 24.5 Currency System 🟢
- [ ] In-game currency
- [ ] Earn currency from matches
- [ ] Spend on cosmetics
- [ ] Premium currency (optional, real money)

---

## Phase 4: Cross-platform & Polish

### Module 25: Mobile Port
> Phiên bản di động iOS/Android

#### 25.1 iOS Build 🔴
- [ ] iOS project setup
- [ ] App Store requirements compliance
- [ ] iOS-specific optimizations
- [ ] TestFlight distribution
- [ ] App Store submission

#### 25.2 Android Build 🔴
- [ ] Android project setup
- [ ] Google Play requirements compliance
- [ ] Android-specific optimizations
- [ ] APK/AAB generation
- [ ] Google Play submission

#### 25.3 Touch Controls 🔴
- [ ] Tap to select piece
- [ ] Tap to move/capture
- [ ] Drag piece movement
- [ ] Pinch to zoom
- [ ] Swipe to pan
- [ ] Double-tap actions
- [ ] Long-press context menu
- [ ] Touch feedback (haptics)

#### 25.4 Responsive UI 🔴
- [ ] Portrait mode support
- [ ] Landscape mode support
- [ ] Dynamic layout scaling
- [ ] Safe area handling (notch, home bar)
- [ ] Font scaling
- [ ] Button size optimization for touch

#### 25.5 Performance Optimization 🔴
- [ ] Mobile GPU optimization
- [ ] Memory usage reduction
- [ ] Battery usage optimization
- [ ] Thermal management
- [ ] Asset quality levels
- [ ] Loading time optimization

#### 25.6 Mobile-specific Features 🟠
- [ ] Push notifications
- [ ] Background game state save
- [ ] App lifecycle handling
- [ ] Mobile social sharing
- [ ] Device-specific optimizations

---

### Module 26: Web Port
> Phiên bản Web

#### 26.1 WebGL Build 🔴
- [ ] WebGL project configuration
- [ ] Build optimization for web
- [ ] Loading progress display
- [ ] First-load optimization

#### 26.2 Browser Compatibility 🔴
- [ ] Chrome support
- [ ] Firefox support
- [ ] Safari support
- [ ] Edge support
- [ ] Mobile browser support
- [ ] Browser version requirements

#### 26.3 Web-specific Adaptations 🔴
- [ ] Keyboard controls
- [ ] Mouse controls
- [ ] Responsive canvas sizing
- [ ] Fullscreen toggle
- [ ] Browser tab handling

#### 26.4 Cloud Saves 🟠
- [ ] Web storage for settings
- [ ] Cloud save integration
- [ ] Cross-session persistence
- [ ] Account-linked saves

#### 26.5 Web Hosting 🔴
- [ ] Static hosting setup
- [ ] CDN configuration
- [ ] Domain setup
- [ ] SSL certificate

---

### Module 27: Cross-platform Play
> Chơi chéo nền tảng

#### 27.1 Unified Backend 🔴
- [ ] Single server for all platforms
- [ ] Platform-agnostic API
- [ ] Platform identification

#### 27.2 Cross-platform Matchmaking 🔴
- [ ] Match players across platforms
- [ ] Platform indicator in UI
- [ ] Platform preference filter (optional)

#### 27.3 Account Sync 🔴
- [ ] Single account across platforms
- [ ] Progress sync
- [ ] Settings sync
- [ ] Cosmetics sync
- [ ] Friends list sync

#### 27.4 Platform Parity 🟠
- [ ] Feature parity across platforms
- [ ] Version sync requirements
- [ ] Graceful degradation for older versions

---

### Module 28: Localization (Optional)
> Đa ngôn ngữ

#### 28.1 Language Support 🟢
- [ ] Vietnamese (primary)
- [ ] English
- [ ] Additional languages (future)

#### 28.2 Localization System 🟢
- [ ] Text localization framework
- [ ] String externalization
- [ ] Language file format (JSON/XML)
- [ ] Dynamic text loading

#### 28.3 UI Localization 🟢
- [ ] Menu text
- [ ] Game UI text
- [ ] Tutorial text
- [ ] Notifications
- [ ] Error messages

#### 28.4 Content Localization 🟢
- [ ] Piece names
- [ ] Rule descriptions
- [ ] Achievement descriptions
- [ ] News/announcements

#### 28.5 Language Settings 🟢
- [ ] Language selection in settings
- [ ] Auto-detect system language
- [ ] Language change without restart

---

### Module 29: Performance & Optimization
> Tối ưu hóa hiệu năng

#### 29.1 Frame Rate Target 🔴
- [ ] 60 FPS target (desktop)
- [ ] 60 FPS target (mobile high-end)
- [ ] 30 FPS option (mobile low-end)
- [ ] VSync options
- [ ] Frame rate limiter

#### 29.2 Memory Management 🔴
- [ ] Object pooling
- [ ] Asset unloading strategy
- [ ] Memory profiling
- [ ] Memory leak prevention
- [ ] Garbage collection optimization

#### 29.3 Asset Optimization 🔴
- [ ] Texture compression
- [ ] Sprite atlasing
- [ ] Audio compression
- [ ] Asset bundle strategy
- [ ] On-demand asset loading

#### 29.4 Loading Optimization 🟠
- [ ] Async loading
- [ ] Loading prioritization
- [ ] Preloading strategy
- [ ] Cold start optimization

#### 29.5 Network Optimization 🟠
- [ ] Data compression
- [ ] Request batching
- [ ] Caching strategy
- [ ] Offline mode support

#### 29.6 Quality Settings 🟠
- [ ] Graphics quality levels (Low/Medium/High)
- [ ] Auto-detect hardware
- [ ] Manual quality selection
- [ ] Per-feature toggles

---

### Module 30: Quality Assurance & Testing
> Đảm bảo chất lượng và kiểm thử

#### 30.1 Unit Tests 🔴
- [ ] Game logic unit tests
- [ ] Movement validation tests
- [ ] Capture validation tests
- [ ] Score calculation tests
- [ ] Win condition tests
- [ ] Special rules tests

#### 30.2 Integration Tests 🔴
- [ ] UI integration tests
- [ ] Network integration tests
- [ ] Database integration tests
- [ ] End-to-end game flow tests

#### 30.3 Playtesting 🔴
- [ ] Internal playtesting sessions
- [ ] External beta testing
- [ ] Feedback collection system
- [ ] Bug report system in-game
- [ ] Analytics integration

#### 30.4 Bug Tracking 🔴
- [ ] Bug tracking system setup
- [ ] Bug severity classification
- [ ] Bug assignment workflow
- [ ] Bug verification process

#### 30.5 Automated Testing 🟠
- [ ] CI/CD pipeline setup
- [ ] Automated test runs on commit
- [ ] Build verification tests
- [ ] Regression testing

#### 30.6 Performance Testing 🟠
- [ ] Frame rate benchmarks
- [ ] Memory benchmarks
- [ ] Load testing (server)
- [ ] Stress testing

---

## Backend Infrastructure (C# Custom Server)

### Module 31: Server Architecture
> Kiến trúc server

#### 31.1 RESTful API 🔴
- [ ] API endpoint design
- [ ] Request/response DTOs
- [ ] API versioning strategy
- [ ] HTTPS enforcement
- [ ] Rate limiting
- [ ] API documentation (Swagger/OpenAPI)

#### 31.2 WebSocket Server 🔴
- [ ] Real-time connection handling
- [ ] Message protocol design
- [ ] Connection management
- [ ] Heartbeat/ping-pong
- [ ] Reconnection handling
- [ ] Message queuing

#### 31.3 Database Design 🔴
- [ ] User table schema
- [ ] Game/Match table schema
- [ ] Replay data schema
- [ ] Friend relationship schema
- [ ] Leaderboard schema
- [ ] Achievement schema
- [ ] Database migrations strategy
- [ ] Indexing strategy

#### 31.4 JWT Authentication 🔴
- [ ] Token generation
- [ ] Token validation
- [ ] Token refresh flow
- [ ] Token revocation
- [ ] Secure token storage advice

#### 31.5 Game Server Instances 🔴
- [ ] Game session management
- [ ] Room management
- [ ] Match state management
- [ ] Server-side game validation
- [ ] Anti-cheat measures

---

### Module 32: Server Features
> Tính năng server

#### 32.1 User Management 🔴
- [ ] User registration endpoint
- [ ] User login endpoint
- [ ] User profile CRUD
- [ ] Password change
- [ ] Account deletion
- [ ] Email verification

#### 32.2 ELO Calculation 🔴
- [ ] ELO algorithm implementation
- [ ] K-factor configuration
- [ ] ELO update on match end
- [ ] ELO history tracking
- [ ] Provisional rating handling

#### 32.3 Match History 🔴
- [ ] Match recording
- [ ] Match retrieval API
- [ ] Match search/filter
- [ ] Match statistics aggregation

#### 32.4 Replay Storage 🔴
- [ ] Replay data format
- [ ] Replay storage (database/file)
- [ ] Replay retrieval API
- [ ] Replay cleanup policy

#### 32.5 Anti-cheat System 🟠
- [ ] Server-side move validation
- [ ] Time manipulation detection
- [ ] Suspicious behavior detection
- [ ] Report handling
- [ ] Ban system

#### 32.6 Admin Tools 🟠
- [ ] Admin dashboard
- [ ] User management (ban, unban)
- [ ] Server monitoring
- [ ] Broadcast messages
- [ ] Server configuration

---

### Module 33: DevOps & Hosting
> Triển khai và vận hành

#### 33.1 Cloud Hosting 🔴
- [ ] AWS/Azure/GCP setup
- [ ] Server provisioning
- [ ] Load balancer configuration
- [ ] Auto-scaling setup (optional)
- [ ] Region selection

#### 33.2 Database Hosting 🔴
- [ ] Managed database service
- [ ] Connection pooling
- [ ] Backup configuration
- [ ] Read replicas (optional)

#### 33.3 Monitoring 🔴
- [ ] Server health monitoring
- [ ] Application metrics
- [ ] Error logging (Sentry/similar)
- [ ] Alerting setup
- [ ] Performance monitoring

#### 33.4 CI/CD Pipeline 🟠
- [ ] Source control (GitHub)
- [ ] Build automation
- [ ] Test automation
- [ ] Deployment automation
- [ ] Rollback capability

#### 33.5 Backups 🔴
- [ ] Automated database backups
- [ ] Backup retention policy
- [ ] Backup restoration testing
- [ ] Disaster recovery plan

#### 33.6 Security 🔴
- [ ] Firewall configuration
- [ ] DDoS protection
- [ ] SSL/TLS certificates
- [ ] Secret management
- [ ] Security audits

---

### Module 34: Cost Optimization Strategies
> Chiến lược tối ưu chi phí

#### 34.1 Caching Strategy 🔴
- [ ] Redis cache setup
- [ ] Leaderboard caching
- [ ] Profile caching
- [ ] Session caching
- [ ] Cache invalidation strategy

#### 34.2 Connection Pooling 🔴
- [ ] Database connection pooling
- [ ] WebSocket connection optimization
- [ ] Connection limits

#### 34.3 Data Compression 🟠
- [ ] API response compression
- [ ] WebSocket message compression
- [ ] Replay data compression
- [ ] Asset compression

#### 34.4 Free Tier Usage 🔴
- [ ] Identify free tier limits
- [ ] Optimize for free tier where possible
- [ ] Cost alerts setup
- [ ] Resource cleanup automation

#### 34.5 Scalability Planning 🟠
- [ ] Horizontal scaling strategy
- [ ] Database sharding plan
- [ ] CDN utilization
- [ ] Serverless options (for appropriate workloads)

---

## Roadmap

### Milestone 1: Core Gameplay + DevMode (3-4 tháng)
> Phase 1 - Features 1-10

- [ ] **Month 1**: Board System, Piece System, Basic Rules
- [ ] **Month 2**: Complete Rules, Standard Mode, Local Multiplayer
- [ ] **Month 3**: UI/UX, Visual Effects, Audio
- [ ] **Month 4**: Tutorial System, DevMode Tools, Polish

**Deliverable**: Playable local multiplayer game with DevMode

---

### Milestone 2: Online Multiplayer (2-3 tháng)
> Phase 2 - Features 11-19

- [ ] **Month 5**: Backend Setup, Authentication, Player Profiles
- [ ] **Month 6**: Room System, Matchmaking, Online Gameplay
- [ ] **Month 7**: Chat, Spectator, Leaderboard, Friend System

**Deliverable**: Full online multiplayer experience

---

### Milestone 3: Advanced Features (1-2 tháng)
> Phase 3 - Features 20-22

- [ ] **Month 8**: Replay System, Custom Setup
- [ ] **Month 9**: Additional Game Modes, Polish

**Deliverable**: Enhanced gameplay with replays and custom modes

---

### Milestone 4: Cross-platform (2-4 tháng)
> Phase 4 - Features 25-30

- [ ] **Month 10-11**: Mobile Port (iOS, Android)
- [ ] **Month 12**: Web Port, Cross-platform Play
- [ ] **Month 13**: Optimization, QA, Polish

**Deliverable**: Multi-platform release

---

### Milestone 5: Content & Polish (Ongoing)
> Optional Features

- [ ] Achievements & Progression (Module 23)
- [ ] Cosmetic Customization (Module 24)
- [ ] Localization (Module 28)
- [ ] Community features
- [ ] Regular content updates

**Deliverable**: Long-term engagement features

---

## Lợi ích của DevMode trong Phase 1

1. **Testing & Debugging** - Dễ dàng test edge cases, reproduce bugs với specific game states
2. **AI Development** - Tools để tạo training scenarios cho AI sau này
3. **Rapid Prototyping** - Thử nghiệm nhanh các biến thể game rules
4. **Content Creation** - Tạo tutorial scenarios, puzzle challenges
5. **Bug Reproduction** - Dễ dàng recreate bugs với specific game states
6. **Community Involvement** - Sau khi release, community có thể tạo custom modes

---

## Technology Stack

### Game Client
| Component | Technology |
|-----------|------------|
| Game Engine | Unity (C#) |
| UI Framework | Unity UI / UI Toolkit |
| Audio | Unity Audio System |
| Input | Unity Input System |
| Platform | Windows, macOS, iOS, Android, WebGL |

### Backend Server
| Component | Technology |
|-----------|------------|
| Server Runtime | .NET 6/7 (C#) |
| Web Framework | ASP.NET Core |
| Real-time | SignalR / Native WebSocket |
| ORM | Entity Framework Core |

### Database & Storage
| Component | Technology |
|-----------|------------|
| Primary Database | PostgreSQL / MySQL |
| Cache | Redis |
| File Storage | AWS S3 / Azure Blob (replays, assets) |

### Infrastructure
| Component | Technology |
|-----------|------------|
| Hosting | AWS / Azure / GCP / DigitalOcean / Linode |
| Container | Docker (optional) |
| CI/CD | GitHub Actions |
| Monitoring | Application Insights / CloudWatch / Prometheus |
| Logging | Serilog + Seq / ELK Stack |

### Communication
| Component | Technology |
|-----------|------------|
| Real-time Protocol | WebSocket |
| API Protocol | REST (JSON) |
| Authentication | JWT |

---

## Ghi chú

### Cách sử dụng file này:
- [ ] = Chưa bắt đầu
- [x] = Hoàn thành
- 🔴🟠🟡🟢 = Priority levels

### Update Guidelines:
- Cập nhật thường xuyên khi có thay đổi requirements
- Mark completed features với [x]
- Thêm notes vào từng feature khi cần

### Tổng kết:
| Metric | Value |
|--------|-------|
| Total Modules | 34 |
| Total Features | 1000+ |
| Phases | 4 |
| Est. Total Time | 12-18 months |

---

*Last Updated: 2025-12-06*
*Version: 1.0*
