import Layout_alt from "@/components/templates/layout"
import { useNavigate } from "react-router-dom"
import { Card, CardDescription, CardTitle } from "./components/ui/card"


function App() {
  const navigate = useNavigate()
  return (
    <Layout_alt>
      <div className="pt-8 pb-4 flex flex-col items-center">

      </div>
      <div className="flex flex-row items-center justify-center gap-15 mb-10">
        {/* Card 1 */}
        <div className="relative flex flex-col items-center">
          <Card onClick={() => navigate("/valgfag")} className="border-0 w-[300px] text-[#d8cdcd] bg-[#19141c] h-[450px] cursor-pointer hover:text-white active:scale-3d hover:scale-110 backdrop-blur-md mt-10 overflow-hidden hover:shadow-blue-500/40 transition-shadow duration-300 shadow-2xl ">
            <div className="relative w-full">
              <img src="/src/components/images/bluey.jpg" alt="Description" className="w-full h-70 object-cover rounded-t-2xl hover:shadow-blue-500/30 transition-shadow duration-300" />
              <div className="absolute left-1/2 -translate-x-1/2 -top-10 w-64 h-16 bg-[#19141c] rounded-b-full z-10"></div>
            </div>
            <CardTitle className=" text-center font-bold text-2xl mb-0">Valgfag</CardTitle>
            <CardDescription className="text-center font-sans text-md">Vælg dit ønskede valgfag her! Du kan se mulighederne.</CardDescription>
          </Card>
        </div>
        {/* Card 2 */}
        <div className="relative flex flex-col items-center">
          <Card className="border-0 w-[300px] text-[#d8cdcd] bg-[#19141c] h-[450px] cursor-pointer hover:text-white active:scale-3d hover:scale-110 backdrop-blur-md mt-10 overflow-hidden hover:shadow-blue-500/40 transition-shadow duration-300 shadow-2xl">
            <div className="relative w-full">
              <img src="/src/components/images/merc.png" alt="Description" className="w-full h-70 object-cover display-block rounded-t-2xl" />
              <div className="absolute left-1/2 -translate-x-1/2 -top-10 w-64 h-16 bg-[#19141c] rounded-b-full z-10"></div>
            </div>
            <CardTitle className="text-center font-bold text-2xl mb-0">Valgfag</CardTitle>
            <CardDescription className="text-center font-sans text-md">Vælg dit ønskede valgfag her! Du kan se mulighederne.</CardDescription>
          </Card>
        </div>
        {/* Card 3 */}
        <div className="relative flex flex-col items-center">
          <Card onClick={() => navigate("/valgfag")} className="border-0 w-[300px] text-[#d8cdcd] bg-[#19141c] h-[450px] cursor-pointer hover:text-white active:scale-3d hover:scale-110 backdrop-blur-md mt-10 overflow-hidden hover:shadow-blue-500/40 transition-shadow duration-300 shadow-2xl ">
            <div className="relative w-full">
              <img src="/src/components/images/bluey.jpg" alt="Description" className="w-full h-70 object-cover display-block rounded-t-2xl" />
              <div className="absolute left-1/2 -translate-x-1/2 -top-10 w-64 h-16 bg-[#19141c] rounded-b-full z-10"></div>
            </div>
            <CardTitle className=" text-center font-bold text-2xl mb-0 ">Valgfag</CardTitle>
            <CardDescription className="text-center font-sans text-md">Vælg dit ønskede valgfag her! Du kan se mulighederne.</CardDescription>
          </Card>
        </div>
      </div>
    </Layout_alt>
  )
}

export default App