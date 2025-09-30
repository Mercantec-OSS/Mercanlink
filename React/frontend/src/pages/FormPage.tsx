import Navbar from "@/components/templates/navbar"
import Footer from "@/components/templates/footer"
import FloatingInput from "@/components/ui/FloatingInput"
import { useState } from "react"

export default function FormPage() {
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [message, setMessage] = useState("");

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setIsSubmitting(true);
        setMessage("");

        const formData = new FormData(e.currentTarget);

        const data = {
            materialType: formData.get('materialType') as string,
            title: formData.get('title') as string,
            description: formData.get('description') as string,
            discordId: formData.get('DiscordId') as string,
            link: formData.get('link') as string || null,
        };

        try {
            const response = await fetch('/api/KnowledgeCenter', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(data),
            });

            if (response.ok) {
                setMessage("Successfully submitted!");
                e.currentTarget.reset(); // Clear the form
            } else {
                const errorText = await response.text();
                setMessage(`Error: ${errorText}`);
            }
        } catch (error) {
            setMessage(`Network error: ${error}`);
        } finally {
            setIsSubmitting(false);
        }
    };
    return (

        <><Navbar></Navbar>
            <div className="min-h-screen flex flex-col bg-gradient-to-br from-[#181c2c] via-[#23263a] to-[#10121a] text-white pt-20">



                <div className="max-w w-3xl  mx-auto bg-[#19141c] rounded-2xl shadow-lg p-8">
                    <h1 className="text-4xl font-extrabold text-center tracking-wide text-white drop-shadow-lg">Form</h1>
                    <div className="border-t border-white/20 my-8"></div>
                    <form className="flex flex-col items-center gap-6" onSubmit={handleSubmit}>
                        {message && (
                            <div className={`w-full md:w-2/3 p-4 rounded-lg text-center ${message.startsWith('Successfully')
                                ? 'bg-green-600/20 text-green-400 border border-green-500/30'
                                : 'bg-red-600/20 text-red-400 border border-red-500/30'
                                }`}>
                                {message}
                            </div>
                        )}
                        <div className="w-full md:w-2/3">
                            {/* Material Type Dropdown */}
                            <div className="relative mb-6">
                                <select
                                    name="materialType"
                                    id="materialType"
                                    className="peer text-white bg-[#23263a] w-full border-b-2 border-blue-400 focus:border-blue-600 outline-none py-2 px-1 transition-all"
                                    required
                                >
                                    <option value="">Vælg materiale type</option>
                                    <option value="blog-post">Blog Post</option>
                                    <option value="video">Video</option>
                                    <option value="artikel">Artikel</option>
                                    <option value="andet">Andet</option>
                                </select>
                                <label
                                    htmlFor="materialType"
                                    className="absolute left-1 -top-4 text-blue-400 text-sm transition-all"
                                >
                                    Materiale type
                                </label>
                            </div>

                            <FloatingInput name="title" label="Titel" required />
                            <FloatingInput name="description" label="Beskrivelse" required />
                            <FloatingInput name="DiscordId" label="DiscordId" required />
                            <FloatingInput name="link" label="Link" type="url" />

                            <button
                                type="submit"
                                disabled={isSubmitting}
                                className="w-full text-center mt-4 bg-gradient-to-r from-blue-500 to-blue-700 text-white py-3 px-4 rounded-lg shadow hover:scale-[1.04] hover:from-blue-600 hover:to-blue-800 transition-all font-bold text-lg tracking-wide disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
                            >
                                {isSubmitting ? 'Submitting...' : 'Tilmeld'}
                            </button>
                        </div>
                    </form>
                </div>
                <Footer />
            </div ></>

    );
}