function [ res ] = FixTime( m )
    startTime = m(1,2);
    len = size(m,1);
    res = [1:len];
    for i=1:len
        res(i) = (m(i,2) - startTime)/10000;
    end
    res = transpose(res);
end

