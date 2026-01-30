import React, { useState, useEffect } from 'react'
import { supabase } from '../services/supabase'

interface RankingEntry {
    id: string
    user_name: string
    song_name: string
    score: number
    max_combo: number
    perfect: number
    great: number
    bad: number
    miss: number
    created_at: string
}

const RankingPage: React.FC = () => {
    const [rankings, setRankings] = useState<RankingEntry[]>([])
    const [loading, setLoading] = useState(true)
    const [selectedSong, setSelectedSong] = useState<'all' | 'Galaxias' | 'Sodapop'>('all')

    useEffect(() => {
        fetchRankings()
    }, [selectedSong])

    const fetchRankings = async () => {
        setLoading(true)
        try {
            let query = supabase
                .from('ranking')
                .select('*')
                .order('score', { ascending: false })
                .limit(10)

            if (selectedSong !== 'all') {
                query = query.eq('song_name', selectedSong)
            }

            const { data, error } = await query

            if (error) {
                console.error('Error fetching rankings:', error)
                setRankings([])
            } else {
                setRankings(data || [])
            }
        } catch (error) {
            console.error('Error:', error)
            setRankings([])
        } finally {
            setLoading(false)
        }
    }

    if (loading) {
        return (
            <div className="min-h-screen bg-black text-white flex items-center justify-center">
                <div className="animate-pulse text-purple-500 text-2xl font-bold">LOADING RANKING...</div>
            </div>
        )
    }

    return (
        <div className="min-h-screen bg-black text-white pt-[220px] pb-20 px-4">
            <div className="max-w-4xl mx-auto">
                <div className="text-center mb-16">
                    <h1 className="text-5xl font-black mb-4 neon-text">GLOBAL RANKING</h1>
                    <p className="text-gray-400">Top players of all time</p>
                </div>

                {/* Song Filter */}
                <div className="flex justify-center gap-4 mb-8">
                    <button
                        onClick={() => setSelectedSong('all')}
                        className={`px-6 py-2 rounded-full font-bold transition-all ${selectedSong === 'all'
                            ? 'bg-purple-600 text-white'
                            : 'bg-gray-800 text-gray-400 hover:bg-gray-700'
                            }`}
                    >
                        ALL SONGS
                    </button>
                    <button
                        onClick={() => setSelectedSong('Galaxias')}
                        className={`px-6 py-2 rounded-full font-bold transition-all ${selectedSong === 'Galaxias'
                            ? 'bg-purple-600 text-white'
                            : 'bg-gray-800 text-gray-400 hover:bg-gray-700'
                            }`}
                    >
                        GALAXIAS
                    </button>
                    <button
                        onClick={() => setSelectedSong('Sodapop')}
                        className={`px-6 py-2 rounded-full font-bold transition-all ${selectedSong === 'Sodapop'
                            ? 'bg-purple-600 text-white'
                            : 'bg-gray-800 text-gray-400 hover:bg-gray-700'
                            }`}
                    >
                        SODAPOP
                    </button>
                </div>

                <div className="bg-gray-900/50 rounded-2xl border border-gray-800 overflow-hidden backdrop-blur-sm">
                    {/* Header */}
                    <div className="grid grid-cols-12 gap-4 p-6 border-b border-gray-800 text-gray-400 font-bold text-sm uppercase tracking-wider">
                        <div className="col-span-1 text-center">Rank</div>
                        <div className="col-span-5 text-center">Player</div>
                        <div className="col-span-3 text-center">Song</div>
                        <div className="col-span-3 text-center">Score</div>
                    </div>

                    {/* List */}
                    <div className="divide-y divide-gray-800">
                        {rankings.length === 0 ? (
                            <div className="p-12 text-center text-gray-500">
                                <p className="text-xl">No rankings yet!</p>
                                <p className="text-sm mt-2">Play a game to be the first on the leaderboard.</p>
                            </div>
                        ) : (
                            rankings.map((entry, index) => (
                                <div
                                    key={entry.id}
                                    className={`grid grid-cols-12 gap-4 p-6 items-center hover:bg-white/5 transition-colors ${index < 3 ? 'bg-purple-900/10' : ''
                                        }`}
                                >
                                    {/* Rank */}
                                    <div className="col-span-1 flex items-center justify-center">
                                        <span
                                            className={`text-2xl font-bold ${index === 0
                                                ? 'text-yellow-400'
                                                : index === 1
                                                    ? 'text-gray-300'
                                                    : index === 2
                                                        ? 'text-amber-600'
                                                        : 'text-gray-500'
                                                }`}
                                        >
                                            {index + 1}
                                        </span>
                                    </div>

                                    {/* Player Name (Centered) */}
                                    <div className="col-span-5 flex items-center justify-center">
                                        <span className={`font-bold text-lg ${index < 3 ? 'text-white' : 'text-gray-300'}`}>
                                            {entry.user_name}
                                        </span>
                                    </div>

                                    {/* Song */}
                                    <div className="col-span-3 text-center">
                                        <span className="text-sm text-gray-400">{entry.song_name}</span>
                                    </div>

                                    {/* Score */}
                                    <div className="col-span-3 text-center font-mono text-xl text-purple-400 font-bold">
                                        {entry.score.toLocaleString()}
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>

                {/* Stats Info */}
                {rankings.length > 0 && (
                    <div className="mt-8 text-center text-gray-500 text-sm">
                        <p>Showing top {rankings.length} players</p>
                    </div>
                )}
            </div>
        </div>
    )
}

export default RankingPage
