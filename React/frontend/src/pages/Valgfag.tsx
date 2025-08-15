import Navbar from "@/components/templates/navbar";
import Footer from "@/components/templates/footer";
import React from "react";
import { Table } from "@/components/ui/table";



const Valgfag: React.FC = () => {
    return (
        <div className="min-h-screen flex flex-col bg-gradient-to-br from-[#181c2c] via-[#23263a] to-[#10121a] text-white">
            <Navbar />
            <main className="flex-1 py-10 px-4">
                <h1 className="text-3xl font-bold text-center mb-8 text-white">Valgfag</h1>

                <div className="w-5xl mx-auto bg-white text-black ">
                    <div className="p-6">


                        <Table className="">
                            <thead>
                                <tr className="border">
                                    <th className="border border-black/50">Valgfag</th>
                                    <th className="border border-black/30">Uge nr.</th>
                                    <th className="border border-black/30">Dato</th>
                                    <th className="border border-black/30">Underviser tag</th>
                                    <th className="border border-black/30">Varighed</th>
                                    <th className="border border-black/30">Anbefalet efter</th>
                                    <th className="border border-black/30">Program</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td className="border border-black/30">Machine Learning</td>
                                    <td className="border border-black/30">8-9</td>
                                    <td className="border border-black/30">17/02 - 28/02</td>
                                    <td className="border border-black/30">MAGS</td>
                                    <td className="border border-black/30">10 dage</td>
                                    <td className="border border-black/30">H4 / H5</td>
                                    <td className="border border-black/30"></td>
                                </tr>
                                <tr>
                                    <td className="border border-black/30">Machine Learning | Kun fysisk</td>
                                    <td className="border border-black/30">37-38</td>
                                    <td className="border border-black/30">08/09 - 19/09</td>
                                    <td className="border border-black/30">Beate (BELJ)</td>
                                    <td className="border border-black/30">10 Dage</td>
                                    <td className="border border-black/30">H2</td>
                                    <td className="border border-black/30"></td>
                                </tr>
                                <tr>
                                    <td className="border border-black/30">Game Design (3 ugers)</td>
                                    <td className="border border-black/30">43, 44 & 45</td>
                                    <td className="border border-black/30">20/10 - 07/11</td>
                                    <td className="border border-black/30">Kasper (KASC)</td>
                                    <td className="border border-black/30">15 dage</td>
                                    <td className="border border-black/30">H1</td>
                                    <td className="border border-black/30"></td>
                                </tr>

                            </tbody>
                        </Table>
                    </div>
                    <div>
                        <form>
                            <div className="p-6">
                                <label className="text-black">Dit navn:</label>
                                <input
                                    type="text"
                                    name="name"
                                    className="text-black mt-1 block w-full border border-gray-300 rounded-md shadow-sm focus:ring focus:ring-opacity-50 placeholder:pl-1"
                                    placeholder="Indtast dit navn"
                                />
                                <label htmlFor="id" className="block text-sm font-medium text-gray-700 ">
                                    Dit ID:
                                </label>
                                <input
                                    type="text"
                                    name="id"
                                    className="text-black mt-1 block w-full border border-gray-300 rounded-md shadow-sm focus:ring focus:ring-opacity-50 placeholder:pl-1"
                                    placeholder="Indtast dit ID"
                                />

                                <label htmlFor="elective" className="block text-sm font-medium text-gray-700">
                                    Vælg dit valgfag:
                                </label>
                                <select id="elective" name="elective" className="text-black mt-1 block w-full border border-gray-300 rounded-md shadow-sm focus:ring focus:ring-opacity-50">
                                    <option value="Machine-Learning">Machine Learning</option>
                                    <option value="Game-Design">Game Design</option>
                                </select>
                                <button type="submit" className="mt-4 bg-blue-500 text-white py-2 px-4 rounded-md shadow hover:bg-blue-600">
                                    Tilmeld
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </main >
            <Footer />
        </div >
    );
};

export default Valgfag;