data = rand(1, 160);
figure;
hold('all');
MarkerOrder = '+o*x';
MarkerIndex = 1;
ColorOrder  = lines(6);  % see "doc colormap"
ColorIndex  = 1;
H = zeros(1, 16);  % Store handles
for i = 1:16
  H(i) = plot(i, data(i), ...
              'Marker', MarkerOrder(MarkerIndex), ...
              'Color', ColorOrder(ColorIndex, :));
  ColorIndex = ColorIndex + 1;
  if ColorIndex > size(ColorIndex, 1)
    ColorIndex  = 1;
    MarkerIndex = MarkerIndex + 1;
    if MarkerIndex > size(MarkerIndex, 2)
       MarkerIndex = 1;
    end
  end
end
legend(H, {'Name1', 'Name2', 'Name3'}); 