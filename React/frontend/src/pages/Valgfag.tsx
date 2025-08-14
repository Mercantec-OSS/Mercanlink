import Navbar from "@/components/templates/navbar";
import Footer from "@/components/templates/footer";
import React from "react";

// ...existing code...
const electives = [
    {
        name: "Web Development",
        description: "Learn to build modern web applications using HTML, CSS, and JavaScript.",
    },
    {
        name: "Game Design",
        description: "Explore the fundamentals of game mechanics and design your own games.",
    },
    {
        name: "Robotics",
        description: "Get hands-on experience with building and programming robots.",
    },
    {
        name: "Digital Art",
        description: "Create stunning digital artwork using various tools and techniques.",
    },
];

const Valgfag: React.FC = () => {
    return (
        <div className="min-h-screen flex flex-col bg-[#101828]">
            <Navbar />
            <main className="flex-1 py-10 px-4">
                <h1 className="text-3xl font-bold text-center mb-8 text-blue-700">Valgfag</h1>
                <div className="max-w-3xl mx-auto grid gap-6 md:grid-cols-2">
                    {electives.map((elective) => (
                        <div
                            key={elective.name}
                            className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition"
                        >
                            <h2 className="text-xl font-semibold text-gray-800 mb-2">{elective.name}</h2>
                            <p className="text-gray-600">{elective.description}</p>
                        </div>
                    ))}
                </div>
            </main>
            <Footer />
        </div>
    );
};

export default Valgfag;