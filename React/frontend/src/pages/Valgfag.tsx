
import Navbar from "@/components/templates/navbar";
import Footer from "@/components/templates/footer";
import React from "react";
import { FaRobot, FaGamepad } from "react-icons/fa";
import { Button } from "@/components/ui/button";
import { useNavigate } from "react-router-dom";

const electives = [
    {
        name: "Machine Learning",
        week: "8-9",
        date: "17/02 - 28/02",
        teacher: "MAGS",
        duration: "10 dage",
        recommended: "H4 / H5",
        icon: <FaRobot className="text-blue-400 text-2xl mr-2" />,
    },
    {
        name: "Machine Learning | Kun fysisk",
        week: "37-38",
        date: "08/09 - 19/09",
        teacher: "Beate (BELJ)",
        duration: "10 dage",
        recommended: "H2",
        icon: <FaRobot className="text-pink-400 text-2xl mr-2" />,
    },
    {
        name: "Game Design (3 ugers)",
        week: "43, 44 & 45",
        date: "20/10 - 07/11",
        teacher: "Kasper (KASC)",
        duration: "15 dage",
        recommended: "H1",
        icon: <FaGamepad className="text-green-400 text-2xl mr-2" />,
    },
];
const Valgfag: React.FC = () => {
    const navigate = useNavigate();
    return (

        <><Navbar></Navbar><div className="min-h-screen flex flex-col bg-gradient-to-br from-[#181c2c] via-[#23263a] to-[#10121a] text-white">


            <h1 className="text-4xl font-extrabold text-center mb-10 tracking-wide text-white drop-shadow-lg">Valgfag</h1><div className="max-w-5xl mx-auto bg-[#19141c] rounded-2xl shadow-lg p-8">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-10">
                    {electives.map((e, i) => (
                        <div
                            key={i}
                            className="flex flex-col items-center bg-gradient-to-tr from-[#23263a] to-[#181c2c] rounded-xl shadow-md p-6 hover:scale-[1.03] transition-transform border border-white/10"
                        >
                            <div className="mb-2">{e.icon}</div>
                            <div className="text-xl font-bold mb-1">{e.name}</div>
                            <div className="text-sm text-white/80 mb-2">Uge: {e.week}</div>
                            <div className="text-sm text-white/80 mb-2">Dato: {e.date}</div>
                            <div className="text-sm text-white/80 mb-2">Underviser: {e.teacher}</div>
                            <div className="text-sm text-white/80 mb-2">Varighed: {e.duration}</div>
                            <div className="text-sm text-white/80 mb-2">Anbefalet efter: {e.recommended}</div>
                        </div>
                    ))}
                </div>
                <div className="border-t border-white/20 my-8"></div>
                <form className="flex flex-col items-center gap-6">
                    <div className="w-full md:w-2/3">
                        <div className="relative mb-6">
                            <input
                                type="text"
                                name="name"
                                id="name"
                                className="peer text-white w-full bg-transparent border-b-2 border-blue-400 focus:border-blue-600 outline-none py-2 px-1 placeholder-transparent transition-all"
                                placeholder="Dit navn"
                                required />
                            <label
                                htmlFor="name"
                                className="absolute left-1 top-2 text-white/70 text-sm transition-all peer-placeholder-shown:top-2 peer-placeholder-shown:text-base peer-focus:-top-4 peer-focus:text-blue-400 peer-focus:text-sm"
                            >Dit navn</label>
                        </div>
                        <div className="relative mb-6">
                            <input
                                type="text"
                                name="id"
                                id="id"
                                className="peer text-white w-full bg-transparent border-b-2 border-blue-400 focus:border-blue-600 outline-none py-2 px-1 placeholder-transparent transition-all"
                                placeholder="Dit ID"
                                required />
                            <label
                                htmlFor="id"
                                className="absolute left-1 top-2 text-white/70 text-sm transition-all peer-placeholder-shown:top-2 peer-placeholder-shown:text-base peer-focus:-top-4 peer-focus:text-blue-400 peer-focus:text-sm"
                            >Dit ID</label>
                        </div>
                        <div className="relative mb-6">
                            <label htmlFor="elective" className="block text-sm font-medium text-white/70 mb-2">
                                Vælg dit valgfag:
                            </label>
                            <select
                                id="elective"
                                name="elective"
                                className="text-white w-full bg-[#23263a] border-b-2 border-blue-400 focus:border-blue-600 outline-none py-2 px-1 rounded-md transition-all"
                                required
                            >
                                <option value="Machine-Learning">Machine Learning</option>
                                <option value="Machine-Learning-Fysisk">Machine Learning (Kun fysisk)</option>
                                <option value="Game-Design">Game Design</option>
                            </select>
                        </div>
                        <button
                            type="submit"
                            className="w-full text-center mt-4 bg-gradient-to-r from-blue-500 to-blue-700 text-white py-3 px-4 rounded-lg shadow hover:scale-[1.04] hover:from-blue-600 hover:to-blue-800 transition-all font-bold text-lg tracking-wide"
                        >
                            Tilmeld
                        </button>
                    </div>
                </form>
            </div>
            <Footer />
        </div></>
    );
};

export default Valgfag;