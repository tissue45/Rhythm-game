# rhythm-game-website

A rhythm game website with phone motion-controlled gameplay.

## Features

- 📱 **Phone Motion Control**: Use your phone's camera to control your avatar
- 🎮 **Interactive Rhythm Gameplay**: Engaging rhythm game mechanics
- 🌙 **Dark Theme**: Modern dark theme with neon aesthetics
- 👤 **User Authentication**: Login and signup functionality
- 🏆 **Ranking System**: Compete with other players
- 📊 **User Profile**: Personal stats and achievements

## Tech Stack

- **Frontend**: React 18 + TypeScript
- **Styling**: CSS3 with modern animations
- **Routing**: React Router v6
- **Build Tool**: Vite
- **State Management**: React Context API

## Getting Started

### Prerequisites

- Node.js (v14 or higher)
- npm or yarn

### Installation

```bash
# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build
```

## Project Structure

```
rhythm-game-website/
├── src/
│   ├── components/     # Reusable UI components
│   │   └── Header.tsx  # Navigation header
│   ├── pages/          # Page components
│   │   ├── HomePage.tsx    # Main game page
│   │   ├── LoginPage.tsx   # User login
│   │   ├── SignupPage.tsx  # User registration
│   │   ├── RankingPage.tsx # Leaderboard
│   │   └── MyPage.tsx      # User profile
│   ├── App.tsx         # Main app component
│   └── main.tsx        # Entry point
├── public/             # Static assets
└── index.html          # HTML template
```

## Pages

- `/` - Home page with game interface
- `/login` - User login
- `/signup` - User registration
- `/ranking` - Global leaderboard
- `/mypage` - User profile and stats

## Development

The app runs on `http://localhost:5173` by default.

### Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## Features in Detail

### Phone Motion Control
Players can use their phone's camera to detect motion and control their in-game avatar, creating an immersive and interactive gaming experience.

### Dark Theme Design
The website features a modern dark theme with neon accents, creating a visually appealing gaming atmosphere.

### User System
- Secure authentication
- Personal profiles
- Achievement tracking
- Game statistics

### Ranking System
Compete with players worldwide and climb the leaderboard.

## Contributing

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Contact

For questions or feedback, please open an issue on GitHub.