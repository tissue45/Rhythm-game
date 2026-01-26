import React, { useState, useEffect } from 'react'

interface Ranker {
    rank: number
    name: string
    score: number
    avatar: string
    change: 'up' | 'down' | 'same'
}

const RankingPage: React.FC = () => {
    const [rankers, setRankers] = useState<Ranker[]>([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        // Simulate fetching ranking data
        setTimeout(() => {
            const mockData: Ranker[] = [
                { rank: 1, name: 'NeonDancer', score: 1250000, avatar: '🕺', change: 'same' },
                { rank: 2, name: 'RhythmMaster', score: 1180000, avatar: '🎧', change: 'up' },
                { rank: 3, name: 'CyberPunk', score: 1150000, avatar: '🤖', change: 'down' },
                { rank: 4, name: 'GrooveQueen', score: 1050000, avatar: '💃', change: 'same' },
                { rank: 5, name: 'BeatBoxer', score: 980000, avatar: '🎤', change: 'up' },
                { rank: 6, name: 'SynthWave', score: 950000, avatar: '🎹', change: 'down' },
                { rank: 7, name: 'RetroGamer', score: 920000, avatar: '🕹️', change: 'same' },
                { rank: 8, name: 'LaserLight', score: 890000, avatar: '⚡', change: 'up' },
                { rank: 9, name: 'BassDrop', score: 850000, avatar: '🔊', change: 'down' },
                { rank: 10, name: 'VibeCheck', score: 820000, avatar: '✨', change: 'same' },
            ]
            setRankers(mockData)
            setLoading(false)
        }, 800)
    }, [])

    if (loading) {
        return (
            <div className="min-h-screen bg-black text-white flex items-center justify-center">
                <div className="animate-pulse text-purple-500 text-2xl font-bold">LOADING RANKING...</div>
            </div>
        )
    }

    return (
        <div className="min-h-screen bg-black text-white py-20 px-4">
            <div className="max-w-4xl mx-auto">
                <div className="text-center mb-16">
                    <h1 className="text-5xl font-black mb-4 neon-text">GLOBAL RANKING</h1>
                    <p className="text-gray-400">Top players of the week</p>
                </div>

                <div className="bg-gray-900/50 rounded-2xl border border-gray-800 overflow-hidden backdrop-blur-sm">
                    {/* Header */}
                    <div className="grid grid-cols-12 gap-4 p-6 border-b border-gray-800 text-gray-400 font-bold text-sm uppercase tracking-wider">
                        <div className="col-span-2 text-center">Rank</div>
                        <div className="col-span-6">Player</div>
                        <div className="col-span-4 text-right">Score</div>
                    </div>

                    {/* List */}
                    <div className="divide-y divide-gray-800">
                        {rankers.map((ranker) => (
                            <div
                                key={ranker.rank}
                                className={`grid grid-cols-12 gap-4 p-6 items-center hover:bg-white/5 transition-colors ${ranker.rank <= 3 ? 'bg-purple-900/10' : ''
                                    }`}
                            >
                                <div className="col-span-2 flex items-center justify-center gap-2">
                                    <span className={`text-2xl font-bold ${ranker.rank === 1 ? 'text-yellow-400' :
                                            ranker.rank === 2 ? 'text-gray-300' :
                                                ranker.rank === 3 ? 'text-amber-600' :
                                                    'text-gray-500'
                                        }`}>
                                        {ranker.rank}
                                    </span>
                                    {ranker.change === 'up' && <span className="text-green-500 text-xs">▲</span>}
                                    {ranker.change === 'down' && <span className="text-red-500 text-xs">▼</span>}
                                    {ranker.change === 'same' && <span className="text-gray-600 text-xs">-</span>}
                                </div>
                                <div className="col-span-6 flex items-center gap-4">
                                    <div className="w-10 h-10 rounded-full bg-gray-800 flex items-center justify-center text-xl">
                                        {ranker.avatar}
                                    </div>
                                    <span className={`font-bold ${ranker.rank <= 3 ? 'text-white' : 'text-gray-300'}`}>
                                        {ranker.name}
                                    </span>
                                </div>
                                <div className="col-span-4 text-right font-mono text-xl text-purple-400 font-bold">
                                    {ranker.score.toLocaleString()}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="mt-12 text-center">
                    <button className="px-8 py-3 bg-gray-800 hover:bg-gray-700 rounded-full text-gray-300 hover:text-white transition-colors border border-gray-700">
                        Load More
                    </button>
                </div>
            </div>
        </div>
    )
}

export default RankingPage
