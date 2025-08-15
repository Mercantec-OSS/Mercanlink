import Layout_alt from "@/components/templates/layout"
import { useNavigate } from "react-router-dom"
import { Card, CardDescription, CardHeader, CardTitle } from "./components/ui/card"


function App() {
  const navigate = useNavigate()
  return (
    <Layout_alt>
      <h1 className="text-4xl font-bold absolute inset-y-190 top-25 ">Mathias</h1>
      <div className="flex flex-row items-center justify-center gap-15">

        <Card onClick={() => navigate("/valgfag")} className="border-0  w-[300px]  bg-white h-[400px] cursor-pointer inset-shadow-slate-800">
          <img src="/src/components/images/bluey.jpg" alt="Description" className="mb-0 w-full h-70 object-cover display-block rounded-t-2xl" />

          <CardTitle className="text-black text-center font-bold text-2xl mb-0 ">Valgfag</CardTitle>
          <CardDescription className="text-black text-center font-sans text-md">Vælg dit ønskede valgfag her! Du kan se mulighederne.</CardDescription>
        </Card>

        <Card className="border-0 width: w-[300px] max-w-sm bg-white top-0 h-[400px]">
          <img src="/src/components/images/merc.png" alt="Description" className="w-full h-70 object-cover display-block rounded-t-2xl" />


          <CardTitle className="text-black">Valgfag</CardTitle>
          <CardDescription>

          </CardDescription>

        </Card>

        <Card className="border-0 width: w-[300px] max-w-sm bg-white top-0 h-[400px]">
          <img src="/src/components/images/bluey.jpg" alt="Description" className="w-full h-70 object-cover display-block rounded-t-2xl" />
          <CardHeader>


            <CardTitle className="text-black">Valgfag</CardTitle>
            <CardDescription>

            </CardDescription>
          </CardHeader>
        </Card>
      </div>
    </Layout_alt>
  )
}

export default App