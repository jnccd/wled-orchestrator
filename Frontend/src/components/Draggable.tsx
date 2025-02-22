import { Box, useColorMode } from "@chakra-ui/react";
import React, { ReactNode, useState } from "react";

const debuggingLogs = false;

interface Props {
  id: string;
  className: string;
  children: ReactNode;
  backgroundColor?: string;
  onDragEnd?: (dragged: HTMLElement, mousePos: number[]) => void;
}

const Draggable = ({
  id,
  children,
  onDragEnd,
  className,
  backgroundColor = "",
}: Props) => {
  const { colorMode } = useColorMode();
  const [dragging, setDragging] = useState(false);
  const [draggingStartPos, setDraggingStartPos] = useState([0, 0]);
  const [draggingLastPos, setDraggingLastPos] = useState([0, 0]);
  const [dragArea, setDragArea] = useState([0, document.body.clientWidth]);
  const dragY = true;

  const dragPointerDown = (e: React.PointerEvent) => {
    if (debuggingLogs) console.log("dragMouseDown");
    if (e.button !== 0) return;

    let thisInDocument = document.getElementById(id);
    if (thisInDocument === null) {
      if (debuggingLogs) console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    const elemsAtPoint = document.elementsFromPoint(e.clientX, e.clientY);
    const filteredElems = elemsAtPoint.filter((x) =>
      x.classList.contains("consumes-click")
    );
    if (filteredElems[0] !== thisInDocument) return;

    // Otherwise the drag becomes a scroll
    elemsAtPoint.forEach((element) => {
      (element as HTMLElement).style.setProperty("touch-action", "none");
    });
    e.stopPropagation();

    dragDown(e.clientX, e.clientY, thisInDocument);
  };

  const dragDown = (x: number, y: number, thisInDocument: HTMLElement) => {
    if (debuggingLogs) console.log("dragDown");

    const parentBounds =
      thisInDocument.parentElement?.parentElement?.parentElement?.getBoundingClientRect(); // TODO: Add prop for number of jumps to bounds giving parent elem
    if (parentBounds) {
      setDragArea([parentBounds.left, parentBounds.right]);
    }

    setDraggingStartPos([
      x - (parseInt(thisInDocument.style.left.split("p")[0]) || 0),
      y - (parseInt(thisInDocument.style.top.split("p")[0]) || 0),
    ]);
    thisInDocument.style.transition = "";
    thisInDocument.style.zIndex = "10";
    if (debuggingLogs) console.log(draggingStartPos);
    setDragging(true);
  };

  const dragPointerMove = (e: PointerEvent) => {
    if (debuggingLogs) console.log("dragMouseMove, " + dragging + id);
    if (!dragging) {
      return;
    }

    e.stopPropagation();

    dragMove(e.clientX, e.clientY);
  };

  const dragMove = (x: number, y: number) => {
    if (debuggingLogs) console.log("dragMove, " + dragging + id);

    let thisInDocument = document.getElementById(id);
    if (thisInDocument === null) {
      if (debuggingLogs) console.log("dragMove, cant find thisInDocument");
      return;
    }

    setPos(thisInDocument, x - draggingStartPos[0], y - draggingStartPos[1]);

    setDraggingLastPos([x, y]);
  };

  const dragUp = () => {
    if (debuggingLogs) console.log("dragMouseUp");
    let thisInDocument = document.getElementById(id);
    if (thisInDocument === null) {
      if (debuggingLogs) console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    const elemsAtPoint = document.elementsFromPoint(
      draggingLastPos[0],
      draggingLastPos[1]
    );
    const filteredElems = elemsAtPoint.filter((x) =>
      x.classList.contains("consumes-click")
    );
    if (filteredElems[0] !== thisInDocument) return;

    elemsAtPoint.forEach((element) => {
      (element as HTMLElement).style.removeProperty("touch-action");
    });

    thisInDocument.style.transition = "top 0.4s, left 0.4s";
    thisInDocument.style.zIndex = "1";
    setPos(thisInDocument, 0, 0);
    if (onDragEnd && dragging) onDragEnd(thisInDocument, draggingLastPos);
    setDragging(false);
  };

  const setPos = (e: HTMLElement, x: number, y: number) => {
    e.style.left = x + "px";
    if (dragY) {
      e.style.top = y + "px";
    }

    const boundingRect = e.getBoundingClientRect();
    if (boundingRect.left < dragArea[0]) {
      if (debuggingLogs) console.log("too far left!");
      const left = parseInt(e.style.left.split("p")[0]);
      e.style.left = String(left + (dragArea[0] - boundingRect.x)) + "px";
    }
    if (boundingRect.right > dragArea[1]) {
      if (debuggingLogs) console.log("too far right!");
      const left = parseInt(e.style.left.split("p")[0]);
      e.style.left = String(left + (dragArea[1] - boundingRect.right)) + "px";
    }
  };

  document.onpointermove = dragPointerMove;
  document.onpointerup = dragUp;

  return (
    <Box
      id={id}
      key={id}
      position="relative"
      className={className + " consumes-click wledServerButton"}
      margin={2}
      paddingX={4}
      backgroundColor={
        backgroundColor !== ""
          ? backgroundColor
          : colorMode === "dark"
          ? "#293042a3"
          : "#f5f9fba3"
      }
      backdropFilter={"auto"}
      backdropBlur={"2px"}
      boxShadow={"rgb(49 110 140 / 64%) 0px 0px 6px -2px"}
      borderRadius={8}
      paddingY={2}
      cursor="move"
      onPointerDown={dragPointerDown}
    >
      {children}
    </Box>
  );
};

export default Draggable;
