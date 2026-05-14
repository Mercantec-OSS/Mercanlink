import { render, screen } from "@testing-library/react"
import { describe, expect, it } from "vitest"

describe("smoke", () => {
  it("renderer et simpelt element", () => {
    render(<p data-testid="label">Mercanlink</p>)
    expect(screen.getByTestId("label")).toHaveTextContent("Mercanlink")
  })
})
