import { Button } from "@chakra-ui/react";
import React, { ReactNode, useState } from "react";

const debuggingLogs = false;

interface Props {
  id: string;
  children: ReactNode;
  onDragEnd?: (elem: HTMLElement, mousePos: number[]) => void;
  className: string;
}

const DraggableButton = ({ id, children, onDragEnd, className }: Props) => {
  const [dragging, setDragging] = useState(false);
  const [draggingStartPos, setDraggingStartPos] = useState([0, 0]);
  const [draggingLastPos, setDraggingLastPos] = useState([0, 0]);
  const [dragArea, setDragArea] = useState([0, document.body.clientWidth]);
  const dragY = true;

  const dragMouseDown = (e: React.MouseEvent) => {
    if (debuggingLogs) console.log("dragMouseDown");
    e.preventDefault();
    if (e.button !== 0) return;

    var thisInDocument = document.getElementById(id);
    if (thisInDocument === null) {
      if (debuggingLogs) console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    const parentBounds =
      thisInDocument.parentElement?.parentElement?.getBoundingClientRect();
    if (parentBounds) {
      setDragArea([parentBounds.left, parentBounds.right]);
    }

    setDraggingStartPos([
      e.clientX - (parseInt(thisInDocument.style.left.split("p")[0]) || 0),
      e.clientY - (parseInt(thisInDocument.style.top.split("p")[0]) || 0),
    ]);
    thisInDocument.style.transition = "";
    thisInDocument.style.zIndex = "10";
    if (debuggingLogs) console.log(draggingStartPos);
    setDragging(true);
  };

  const dragMouseMove = (e: MouseEvent) => {
    if (debuggingLogs) console.log("dragMouseMove, " + dragging + id);
    if (!dragging) {
      return;
    }

    e.preventDefault();

    var thisInDocument = document.getElementById(id);
    if (thisInDocument === null) {
      if (debuggingLogs) console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    setPos(
      thisInDocument,
      e.clientX - draggingStartPos[0],
      e.clientY - draggingStartPos[1]
    );

    setDraggingLastPos([e.clientX, e.clientY]);
  };

  const dragMouseUp = () => {
    if (debuggingLogs) console.log("dragMouseUp");
    var thisInDocument = document.getElementById(id);
    if (thisInDocument === null) {
      if (debuggingLogs) console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    thisInDocument.style.transition = "left 0.4s";
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

  document.onmousemove = dragMouseMove;
  document.onmouseup = dragMouseUp;

  return (
    <Button
      id={id}
      position="relative"
      className={className + " wledServerButton"}
      margin={2}
      size="lg"
      cursor="move"
      onMouseDown={dragMouseDown} // TODO: Add touch event
    >
      {children}
    </Button>
  );
};

export default DraggableButton;
